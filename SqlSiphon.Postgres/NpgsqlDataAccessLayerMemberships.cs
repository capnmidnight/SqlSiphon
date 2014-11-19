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
    public partial class NpgsqlDataAccessLayer
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
        public int aspnet_UsersInRoles_RemoveUsersFromRoles([MappedParameter(Size = 256)]string ApplicationName, string UserNames, string RoleNames)
        {
            return this.Return<int>(ApplicationName, UserNames, RoleNames);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query =
@"DECLARE @ApplicationId uniqueidentifier
SELECT  @ApplicationId = NULL
SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
IF (@ApplicationId IS NULL)
    RETURN(2)
DECLARE @UserId uniqueidentifier
SELECT  @UserId = NULL
DECLARE @RoleId uniqueidentifier
SELECT  @RoleId = NULL

SELECT  @UserId = UserId
FROM    dbo.aspnet_Users
WHERE   LoweredUserName = LOWER(@UserName) AND ApplicationId = @ApplicationId

IF (@UserId IS NULL)
    RETURN(2)

SELECT  @RoleId = RoleId
FROM    dbo.aspnet_Roles
WHERE   LoweredRoleName = LOWER(@RoleName) AND ApplicationId = @ApplicationId

IF (@RoleId IS NULL)
    RETURN(3)

IF (EXISTS( SELECT * FROM dbo.aspnet_UsersInRoles WHERE  UserId = @UserId AND RoleId = @RoleId))
    RETURN(1)
ELSE
    RETURN(0)")]
        public int aspnet_UsersInRoles_IsUserInRole(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string UserName,
            [MappedParameter(Size = 256)]string RoleName)
        {
            return this.Return<int>(ApplicationName, UserName, RoleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query =
@"DECLARE @ApplicationId uniqueidentifier
SELECT  @ApplicationId = NULL
SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
IF (@ApplicationId IS NULL)
    RETURN(1)
DECLARE @RoleId uniqueidentifier
SELECT  @RoleId = NULL

SELECT  @RoleId = RoleId
FROM    dbo.aspnet_Roles
WHERE   LOWER(@RoleName) = LoweredRoleName AND ApplicationId = @ApplicationId

IF (@RoleId IS NULL)
    RETURN(1)

SELECT u.UserName
FROM   dbo.aspnet_Users u, dbo.aspnet_UsersInRoles ur
WHERE  u.UserId = ur.UserId AND @RoleId = ur.RoleId AND u.ApplicationId = @ApplicationId
ORDER BY u.UserName
RETURN(0)")]
        public List<string> aspnet_UsersInRoles_GetUsersInRoles(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName)
        {
            return this.GetList<string>("UserName", ApplicationName, RoleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query =
@"DECLARE @ApplicationId uniqueidentifier
SELECT  @ApplicationId = NULL
SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
IF (@ApplicationId IS NULL)
    RETURN(1)
DECLARE @UserId uniqueidentifier
SELECT  @UserId = NULL

SELECT  @UserId = UserId
FROM    dbo.aspnet_Users
WHERE   LoweredUserName = LOWER(@UserName) AND ApplicationId = @ApplicationId

IF (@UserId IS NULL)
    RETURN(1)

SELECT r.RoleName
FROM   dbo.aspnet_Roles r, dbo.aspnet_UsersInRoles ur
WHERE  r.RoleId = ur.RoleId AND r.ApplicationId = @ApplicationId AND ur.UserId = @UserId
ORDER BY r.RoleName
RETURN (0)")]
        public List<string> aspnet_UsersInRoles_GetRolesForUser(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string UserName)
        {
            return this.GetList<string>("RoleName", ApplicationName, UserName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query =
@"DECLARE @ApplicationId uniqueidentifier
SELECT  @ApplicationId = NULL
SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
IF (@ApplicationId IS NULL)
    RETURN(1)
    DECLARE @RoleId uniqueidentifier
    SELECT  @RoleId = NULL

    SELECT  @RoleId = RoleId
    FROM    dbo.aspnet_Roles
    WHERE   LOWER(@RoleName) = LoweredRoleName AND ApplicationId = @ApplicationId

    IF (@RoleId IS NULL)
        RETURN(1)

SELECT u.UserName
FROM   dbo.aspnet_Users u, dbo.aspnet_UsersInRoles ur
WHERE  u.UserId = ur.UserId AND @RoleId = ur.RoleId AND u.ApplicationId = @ApplicationId AND LoweredUserName LIKE LOWER(@UserNameToMatch)
ORDER BY u.UserName
RETURN(0)")]
        public List<string> aspnet_UsersInRoles_FindUsersInRole(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName,
            [MappedParameter(Size = 256)]string UserNameToMatch)
        {
            return this.GetList<string>("UserName", ApplicationName, RoleName, UserNameToMatch);
        }

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

DECLARE @tbNames	table(Name nvarchar(256) NOT NULL PRIMARY KEY)
DECLARE @tbRoles	table(RoleId uniqueidentifier NOT NULL PRIMARY KEY)
DECLARE @tbUsers	table(UserId uniqueidentifier NOT NULL PRIMARY KEY)
DECLARE @Num		int
DECLARE @Pos		int
DECLARE @NextPos	int
DECLARE @Name		nvarchar(256)

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
	FROM   dbo.aspnet_Roles ar, @tbNames t
	WHERE  LOWER(t.Name) = ar.LoweredRoleName AND ar.ApplicationId = @AppId

IF (@@ROWCOUNT <> @Num)
BEGIN
	SELECT TOP 1 Name
	FROM   @tbNames
	WHERE  LOWER(Name) NOT IN (SELECT ar.LoweredRoleName FROM dbo.aspnet_Roles ar,  @tbRoles r WHERE r.RoleId = ar.RoleId)
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
	FROM   dbo.aspnet_Users ar, @tbNames t
	WHERE  LOWER(t.Name) = ar.LoweredUserName AND ar.ApplicationId = @AppId

IF (@@ROWCOUNT <> @Num)
BEGIN
	DELETE FROM @tbNames
	WHERE LOWER(Name) IN (SELECT LoweredUserName FROM dbo.aspnet_Users au,  @tbUsers u WHERE au.UserId = u.UserId)

	INSERT dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate)
		SELECT @AppId, NEWID(), Name, LOWER(Name), 0, @CurrentTimeUtc
		FROM   @tbNames

	INSERT INTO @tbUsers
		SELECT  UserId
		FROM	dbo.aspnet_Users au, @tbNames t
		WHERE   LOWER(t.Name) = au.LoweredUserName AND au.ApplicationId = @AppId
END

IF (EXISTS (SELECT * FROM dbo.aspnet_UsersInRoles ur, @tbUsers tu, @tbRoles tr WHERE tu.UserId = ur.UserId AND tr.RoleId = ur.RoleId))
BEGIN
	SELECT TOP 1 UserName, RoleName
	FROM		 dbo.aspnet_UsersInRoles ur, @tbUsers tu, @tbRoles tr, aspnet_Users u, aspnet_Roles r
	WHERE		u.UserId = tu.UserId AND r.RoleId = tr.RoleId AND tu.UserId = ur.UserId AND tr.RoleId = ur.RoleId

	IF( @TranStarted = 1 )
		ROLLBACK TRANSACTION
	RETURN(3)
END

INSERT INTO dbo.aspnet_UsersInRoles (UserId, RoleId)
SELECT UserId, RoleId
FROM @tbUsers, @tbRoles

IF( @TranStarted = 1 )
	COMMIT TRANSACTION
RETURN(0)")]
        public int aspnet_UsersInRoles_AddUsersToRoles(
            [MappedParameter(Size = 256)]string ApplicationName,
            string UserNames,
            string RoleNames,
            DateTime CurrentTimeUtc)
        {
            return this.Return<int>(ApplicationName, UserNames, RoleNames, CurrentTimeUtc);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query =
@"DECLARE @UserId               uniqueidentifier
SELECT  @UserId               = NULL
SELECT  @NumTablesDeletedFrom = 0

DECLARE @TranStarted   bit
SET @TranStarted = 0

IF( @@TRANCOUNT = 0 )
BEGIN
	BEGIN TRANSACTION
	SET @TranStarted = 1
END
ELSE
SET @TranStarted = 0

DECLARE @ErrorCode   int
DECLARE @RowCount    int

SET @ErrorCode = 0
SET @RowCount  = 0

SELECT  @UserId = u.UserId
FROM    dbo.aspnet_Users u, dbo.aspnet_Applications a
WHERE   u.LoweredUserName       = LOWER(@UserName)
    AND u.ApplicationId         = a.ApplicationId
    AND LOWER(@ApplicationName) = a.LoweredApplicationName

IF (@UserId IS NULL)
BEGIN
    GOTO Cleanup
END

-- Delete from Membership table if (@TablesToDeleteFrom & 1) is set
IF ((@TablesToDeleteFrom & 1) <> 0 AND
    (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_MembershipUsers') AND (type = 'V'))))
BEGIN
    DELETE FROM dbo.aspnet_Membership WHERE @UserId = UserId

    SELECT @ErrorCode = @@ERROR,
            @RowCount = @@ROWCOUNT

    IF( @ErrorCode <> 0 )
        GOTO Cleanup

    IF (@RowCount <> 0)
        SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
END

-- Delete from aspnet_UsersInRoles table if (@TablesToDeleteFrom & 2) is set
IF ((@TablesToDeleteFrom & 2) <> 0  AND
    (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_UsersInRoles') AND (type = 'V'))) )
BEGIN
    DELETE FROM dbo.aspnet_UsersInRoles WHERE @UserId = UserId

    SELECT @ErrorCode = @@ERROR,
            @RowCount = @@ROWCOUNT

    IF( @ErrorCode <> 0 )
        GOTO Cleanup

    IF (@RowCount <> 0)
        SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
END

-- Delete from aspnet_Profile table if (@TablesToDeleteFrom & 4) is set
IF ((@TablesToDeleteFrom & 4) <> 0  AND
    (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_Profiles') AND (type = 'V'))) )
BEGIN
    DELETE FROM dbo.aspnet_Profile WHERE @UserId = UserId

    SELECT @ErrorCode = @@ERROR,
            @RowCount = @@ROWCOUNT

    IF( @ErrorCode <> 0 )
        GOTO Cleanup

    IF (@RowCount <> 0)
        SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
END

-- Delete from aspnet_PersonalizationPerUser table if (@TablesToDeleteFrom & 8) is set
IF ((@TablesToDeleteFrom & 8) <> 0  AND
    (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_WebPartState_User') AND (type = 'V'))) )
BEGIN
    DELETE FROM dbo.aspnet_PersonalizationPerUser WHERE @UserId = UserId

    SELECT @ErrorCode = @@ERROR,
            @RowCount = @@ROWCOUNT

    IF( @ErrorCode <> 0 )
        GOTO Cleanup

    IF (@RowCount <> 0)
        SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
END

-- Delete from aspnet_Users table if (@TablesToDeleteFrom & 1,2,4 & 8) are all set
IF ((@TablesToDeleteFrom & 1) <> 0 AND
    (@TablesToDeleteFrom & 2) <> 0 AND
    (@TablesToDeleteFrom & 4) <> 0 AND
    (@TablesToDeleteFrom & 8) <> 0 AND
    (EXISTS (SELECT UserId FROM dbo.aspnet_Users WHERE @UserId = UserId)))
BEGIN
    DELETE FROM dbo.aspnet_Users WHERE @UserId = UserId

    SELECT @ErrorCode = @@ERROR,
            @RowCount = @@ROWCOUNT

    IF( @ErrorCode <> 0 )
        GOTO Cleanup

    IF (@RowCount <> 0)
        SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
END

IF( @TranStarted = 1 )
BEGIN
	SET @TranStarted = 0
	COMMIT TRANSACTION
END

RETURN 0

Cleanup:
SET @NumTablesDeletedFrom = 0

IF( @TranStarted = 1 )
BEGIN
    SET @TranStarted = 0
	ROLLBACK TRANSACTION
END

RETURN @ErrorCode")]
        public int aspnet_Users_DeleteUser(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string UserName,
            int TablesToDeleteFrom,
            out int NumTablesDeletedFrom)
        {
            NumTablesDeletedFrom = 0;
            var ps = new object[] { ApplicationName, UserName, TablesToDeleteFrom, NumTablesDeletedFrom };
            var returnCode = this.Return<int>(ps);
            NumTablesDeletedFrom = (int)ps[3];
            return returnCode;
        }



        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query =
@"IF( @UserId IS NULL )
    SELECT @UserId = NEWID()
ELSE
BEGIN
    IF( EXISTS( SELECT UserId FROM dbo.aspnet_Users
                WHERE @UserId = UserId ) )
        RETURN -1
END

INSERT dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate)
VALUES (@ApplicationId, @UserId, @UserName, LOWER(@UserName), @IsUserAnonymous, @LastActivityDate)

RETURN 0")]
        public int aspnet_Users_CreateUser(
            Guid ApplicationId,
            [MappedParameter(Size = 256)]string UserName,
            bool IsUserAnonymous,
            DateTime LastActivityDate,
            out Guid UserId)
        {
            UserId = Guid.Empty;
            var ps = new object[] { ApplicationId, UserName, IsUserAnonymous, LastActivityDate, UserId };
            var returnCode = this.Return<int>(ps);
            UserId = (Guid)ps.Last();
            return returnCode;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query=
@"DECLARE @ApplicationId uniqueidentifier
SELECT  @ApplicationId = NULL
SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
IF (@ApplicationId IS NULL)
    RETURN(0)
IF (EXISTS (SELECT RoleName FROM dbo.aspnet_Roles WHERE LOWER(@RoleName) = LoweredRoleName AND ApplicationId = @ApplicationId ))
    RETURN(1)
ELSE
    RETURN(0)")]
        public int aspnet_Roles_RoleExists(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName)
        {
            return this.Return<int>(ApplicationName, RoleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query=
@"DECLARE @ApplicationId uniqueidentifier
SELECT  @ApplicationId = NULL
SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
IF (@ApplicationId IS NULL)
    RETURN
SELECT RoleName
FROM   dbo.aspnet_Roles WHERE ApplicationId = @ApplicationId
ORDER BY RoleName")]
        public List<string> aspnet_Roles_GetAllRoles(string ApplicationName)
        {
            return this.GetList<string>("RoleName", ApplicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query =
@"DECLARE @ApplicationId uniqueidentifier
SELECT  @ApplicationId = NULL
SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
IF (@ApplicationId IS NULL)
    RETURN(1)

DECLARE @ErrorCode     int
SET @ErrorCode = 0

DECLARE @TranStarted   bit
SET @TranStarted = 0

IF( @@TRANCOUNT = 0 )
BEGIN
    BEGIN TRANSACTION
    SET @TranStarted = 1
END
ELSE
    SET @TranStarted = 0

DECLARE @RoleId   uniqueidentifier
SELECT  @RoleId = NULL
SELECT  @RoleId = RoleId FROM dbo.aspnet_Roles WHERE LoweredRoleName = LOWER(@RoleName) AND ApplicationId = @ApplicationId

IF (@RoleId IS NULL)
BEGIN
    SELECT @ErrorCode = 1
    GOTO Cleanup
END
IF (@DeleteOnlyIfRoleIsEmpty <> 0)
BEGIN
    IF (EXISTS (SELECT RoleId FROM dbo.aspnet_UsersInRoles  WHERE @RoleId = RoleId))
    BEGIN
        SELECT @ErrorCode = 2
        GOTO Cleanup
    END
END


DELETE FROM dbo.aspnet_UsersInRoles  WHERE @RoleId = RoleId

IF( @@ERROR <> 0 )
BEGIN
    SET @ErrorCode = -1
    GOTO Cleanup
END

DELETE FROM dbo.aspnet_Roles WHERE @RoleId = RoleId  AND ApplicationId = @ApplicationId

IF( @@ERROR <> 0 )
BEGIN
    SET @ErrorCode = -1
    GOTO Cleanup
END

IF( @TranStarted = 1 )
BEGIN
    SET @TranStarted = 0
    COMMIT TRANSACTION
END

RETURN(0)

Cleanup:

IF( @TranStarted = 1 )
BEGIN
    SET @TranStarted = 0
    ROLLBACK TRANSACTION
END

RETURN @ErrorCode")]
        public int aspnet_Roles_DeleteRole(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName,
            bool DeleteOnlyIfRoleIsEmpty)
        {
            return this.Return<int>(ApplicationName, RoleName, DeleteOnlyIfRoleIsEmpty);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query=@"
DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

    EXEC dbo.aspnet_Applications_CreateApplication @ApplicationName, @ApplicationId OUTPUT

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (EXISTS(SELECT RoleId FROM dbo.aspnet_Roles WHERE LoweredRoleName = LOWER(@RoleName) AND ApplicationId = @ApplicationId))
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    INSERT INTO dbo.aspnet_Roles
                (ApplicationId, RoleName, LoweredRoleName)
         VALUES (@ApplicationId, @RoleName, LOWER(@RoleName))

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode")]
        public int aspnet_Roles_CreateRole(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName)
        {
            return this.Return<int>(ApplicationName, RoleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(Query=
@"DECLARE @UserId                                 uniqueidentifier
DECLARE @IsApproved                             bit
DECLARE @IsLockedOut                            bit
DECLARE @LastLockoutDate                        datetime
DECLARE @FailedPasswordAttemptCount             int
DECLARE @FailedPasswordAttemptWindowStart       datetime
DECLARE @FailedPasswordAnswerAttemptCount       int
DECLARE @FailedPasswordAnswerAttemptWindowStart datetime

DECLARE @ErrorCode     int
SET @ErrorCode = 0

DECLARE @TranStarted   bit
SET @TranStarted = 0

IF( @@TRANCOUNT = 0 )
BEGIN
	BEGIN TRANSACTION
	SET @TranStarted = 1
END
ELSE
    SET @TranStarted = 0

SELECT  @UserId = u.UserId,
        @IsApproved = m.IsApproved,
        @IsLockedOut = m.IsLockedOut,
        @LastLockoutDate = m.LastLockoutDate,
        @FailedPasswordAttemptCount = m.FailedPasswordAttemptCount,
        @FailedPasswordAttemptWindowStart = m.FailedPasswordAttemptWindowStart,
        @FailedPasswordAnswerAttemptCount = m.FailedPasswordAnswerAttemptCount,
        @FailedPasswordAnswerAttemptWindowStart = m.FailedPasswordAnswerAttemptWindowStart
FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m WITH ( UPDLOCK )
WHERE   LOWER(@ApplicationName) = a.LoweredApplicationName AND
        u.ApplicationId = a.ApplicationId    AND
        u.UserId = m.UserId AND
        LOWER(@UserName) = u.LoweredUserName

IF ( @@rowcount = 0 )
BEGIN
    SET @ErrorCode = 1
    GOTO Cleanup
END

IF( @IsLockedOut = 1 )
BEGIN
    GOTO Cleanup
END

IF( @IsPasswordCorrect = 0 )
BEGIN
    IF( @CurrentTimeUtc > DATEADD( minute, @PasswordAttemptWindow, @FailedPasswordAttemptWindowStart ) )
    BEGIN
        SET @FailedPasswordAttemptWindowStart = @CurrentTimeUtc
        SET @FailedPasswordAttemptCount = 1
    END
    ELSE
    BEGIN
        SET @FailedPasswordAttemptWindowStart = @CurrentTimeUtc
        SET @FailedPasswordAttemptCount = @FailedPasswordAttemptCount + 1
    END

    BEGIN
        IF( @FailedPasswordAttemptCount >= @MaxInvalidPasswordAttempts )
        BEGIN
            SET @IsLockedOut = 1
            SET @LastLockoutDate = @CurrentTimeUtc
        END
    END
END
ELSE
BEGIN
    IF( @FailedPasswordAttemptCount > 0 OR @FailedPasswordAnswerAttemptCount > 0 )
    BEGIN
        SET @FailedPasswordAttemptCount = 0
        SET @FailedPasswordAttemptWindowStart = CONVERT( datetime, '17540101', 112 )
        SET @FailedPasswordAnswerAttemptCount = 0
        SET @FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 )
        SET @LastLockoutDate = CONVERT( datetime, '17540101', 112 )
    END
END

IF( @UpdateLastLoginActivityDate = 1 )
BEGIN
    UPDATE  dbo.aspnet_Users
    SET     LastActivityDate = @LastActivityDate
    WHERE   @UserId = UserId

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    UPDATE  dbo.aspnet_Membership
    SET     LastLoginDate = @LastLoginDate
    WHERE   UserId = @UserId

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END
END


UPDATE dbo.aspnet_Membership
SET IsLockedOut = @IsLockedOut, LastLockoutDate = @LastLockoutDate,
    FailedPasswordAttemptCount = @FailedPasswordAttemptCount,
    FailedPasswordAttemptWindowStart = @FailedPasswordAttemptWindowStart,
    FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount,
    FailedPasswordAnswerAttemptWindowStart = @FailedPasswordAnswerAttemptWindowStart
WHERE @UserId = UserId

IF( @@ERROR <> 0 )
BEGIN
    SET @ErrorCode = -1
    GOTO Cleanup
END

IF( @TranStarted = 1 )
BEGIN
SET @TranStarted = 0
COMMIT TRANSACTION
END

RETURN @ErrorCode

Cleanup:

IF( @TranStarted = 1 )
BEGIN
    SET @TranStarted = 0
    ROLLBACK TRANSACTION
END

RETURN @ErrorCode")]
        public int aspnet_Membership_UpdateUserInfo(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string UserName,
            bool IsPasswordCorrect,
            bool UpdateLastLoginActivityDate,
            int MaxInvalidPasswordAttempts,
            int PasswordAttemptWindow,
            DateTime CurrentTimeUtc,
            DateTime LastLoginDate,
            DateTime LastActivityDate)
        {
            return this.Return<int>(ApplicationName, UserName, IsPasswordCorrect, UpdateLastLoginActivityDate, MaxInvalidPasswordAttempts, PasswordAttemptWindow, CurrentTimeUtc, LastLoginDate, LastActivityDate);
        }
    }
}
