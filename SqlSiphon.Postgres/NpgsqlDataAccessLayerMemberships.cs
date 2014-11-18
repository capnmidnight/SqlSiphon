/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009, 2010, 2011, 2012, 2013 Sean T. McBeth
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this 
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or 
  other materials provided with the distribution.

* Neither the name of McBeth Software Systems nor the names of its contributors
  may be used to endorse or promote products derived from this software without 
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Npgsql;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres
{
    public abstract partial class NpgsqlDataAccessLayer
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query =
@"DECLARE @AppId uniqueidentifier
SELECT  @AppId = NULL
SELECT  @AppId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
IF (@AppId IS NULL)
	RETURN(2)


DECLARE @TranStarted   bit
SET @TranStarted = 0

IF( @@TRANCOUNT = 0 )
BEGIN
	BEGIN TRANSACTION
	SET @TranStarted = 1
END

DECLARE @tbNames  table(Name nvarchar(256) NOT NULL PRIMARY KEY)
DECLARE @tbRoles  table(RoleId uniqueidentifier NOT NULL PRIMARY KEY)
DECLARE @tbUsers  table(UserId uniqueidentifier NOT NULL PRIMARY KEY)
DECLARE @Num	  int
DECLARE @Pos	  int
DECLARE @NextPos  int
DECLARE @Name	  nvarchar(256)
DECLARE @CountAll int
DECLARE @CountU	  int
DECLARE @CountR	  int


SET @Num = 0
SET @Pos = 1
WHILE(@Pos <= LEN(@RoleNames))
BEGIN
	SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
	IF (@NextPos = 0 OR @NextPos IS NULL)
		SELECT @NextPos = LEN(@RoleNames) + 1
	SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
	SELECT @Pos = @NextPos+1

	INSERT INTO @tbNames VALUES (@Name)
	SET @Num = @Num + 1
END

INSERT INTO @tbRoles
	SELECT RoleId
	FROM   public.aspnet_Roles ar, @tbNames t
	WHERE  LOWER(t.Name) = ar.LoweredRoleName AND ar.ApplicationId = @AppId
SELECT @CountR = @@ROWCOUNT

IF (@CountR <> @Num)
BEGIN
	SELECT TOP 1 N'', Name
	FROM   @tbNames
	WHERE  LOWER(Name) NOT IN (SELECT ar.LoweredRoleName FROM public.aspnet_Roles ar,  @tbRoles r WHERE r.RoleId = ar.RoleId)
	IF( @TranStarted = 1 )
		ROLLBACK TRANSACTION
	RETURN(2)
END


DELETE FROM @tbNames WHERE 1=1
SET @Num = 0
SET @Pos = 1


WHILE(@Pos <= LEN(@UserNames))
BEGIN
	SELECT @NextPos = CHARINDEX(N',', @UserNames,  @Pos)
	IF (@NextPos = 0 OR @NextPos IS NULL)
		SELECT @NextPos = LEN(@UserNames) + 1
	SELECT @Name = RTRIM(LTRIM(SUBSTRING(@UserNames, @Pos, @NextPos - @Pos)))
	SELECT @Pos = @NextPos+1

	INSERT INTO @tbNames VALUES (@Name)
	SET @Num = @Num + 1
END

INSERT INTO @tbUsers
	SELECT UserId
	FROM   public.aspnet_Users ar, @tbNames t
	WHERE  LOWER(t.Name) = ar.LoweredUserName AND ar.ApplicationId = @AppId

SELECT @CountU = @@ROWCOUNT
IF (@CountU <> @Num)
BEGIN
	SELECT TOP 1 Name, N''
	FROM   @tbNames
	WHERE  LOWER(Name) NOT IN (SELECT au.LoweredUserName FROM public.aspnet_Users au,  @tbUsers u WHERE u.UserId = au.UserId)

	IF( @TranStarted = 1 )
		ROLLBACK TRANSACTION
	RETURN(1)
END

SELECT  @CountAll = COUNT(*)
FROM	public.aspnet_UsersInRoles ur, @tbUsers u, @tbRoles r
WHERE   ur.UserId = u.UserId AND ur.RoleId = r.RoleId

IF (@CountAll <> @CountU * @CountR)
BEGIN
	SELECT TOP 1 UserName, RoleName
	FROM		 @tbUsers tu, @tbRoles tr, public.aspnet_Users u, public.aspnet_Roles r
	WHERE		 u.UserId = tu.UserId AND r.RoleId = tr.RoleId AND
					tu.UserId NOT IN (SELECT ur.UserId FROM public.aspnet_UsersInRoles ur WHERE ur.RoleId = tr.RoleId) AND
					tr.RoleId NOT IN (SELECT ur.RoleId FROM public.aspnet_UsersInRoles ur WHERE ur.UserId = tu.UserId)
	IF( @TranStarted = 1 )
		ROLLBACK TRANSACTION
	RETURN(3)
END

DELETE FROM public.aspnet_UsersInRoles
WHERE UserId IN (SELECT UserId FROM @tbUsers)
	AND RoleId IN (SELECT RoleId FROM @tbRoles)
IF( @TranStarted = 1 )
	COMMIT TRANSACTION
RETURN(0)")]
        public int aspnet_UsersInRoles_RemoveUsersFromRoles(string ApplicationName, string UserNames, string RoleNames)
        {
            return this.Return<int>(ApplicationName, UserNames, RoleNames);
        }
    }
}
