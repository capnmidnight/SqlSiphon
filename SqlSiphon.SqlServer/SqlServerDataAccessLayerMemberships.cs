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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public partial class SqlServerDataAccessLayer
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public int aspnet_UsersInRoles_RemoveUsersFromRoles(
            [MappedParameter(Size = 256)]string ApplicationName,
            string UserNames,
            string RoleNames)
        {
            return this.Return<int>(ApplicationName, UserNames, RoleNames);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public int aspnet_UsersInRoles_IsUserInRole(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string UserName,
            [MappedParameter(Size = 256)]string RoleName)
        {
            return this.Return<int>(ApplicationName, UserName, RoleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public List<string> aspnet_UsersInRoles_GetUsersInRoles(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName)
        {
            return this.GetList<string>("UserName", ApplicationName, RoleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public List<string> aspnet_UsersInRoles_GetRolesForUser(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string UserName)
        {
            return this.GetList<string>("UserName", ApplicationName, UserName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public List<string> aspnet_UsersInRoles_FindUsersInRole(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName,
            [MappedParameter(Size = 256)]string UserNameToMatch)
        {
            return this.GetList<string>("UserName", ApplicationName, RoleName, UserNameToMatch);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public int aspnet_UsersInRoles_AddUsersToRoles(
            [MappedParameter(Size = 256)]string ApplicationName,
            string UserNames,
            string RoleNames,
            DateTime CurrentTimeUtc)
        {
            return this.Return<int>(ApplicationName, UserNames, RoleNames, CurrentTimeUtc);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public int aspnet_Users_DeleteUser(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string UserName,
            int TablesToDeleteFrom,
            out int NumTablesDeletedFrom)
        {
            NumTablesDeletedFrom = 0;
            var ps = new object[] { ApplicationName, UserName, TablesToDeleteFrom, NumTablesDeletedFrom };
            var returnCode = this.Return<int>(ps);
            NumTablesDeletedFrom = (int)ps.Last();
            return returnCode;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
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
        [MappedMethod]
        public int aspnet_Roles_RoleExists(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName)
        {
            return this.Return<int>(ApplicationName, RoleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public List<string> aspnet_Roles_GetAllRoles([MappedParameter(Size = 256)]string ApplicationName)
        {
            return this.GetList<string>("RoleName", ApplicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public int aspnet_Roles_DeleteRole(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName,
            bool DeleteOnlyIfRoleIsEmpty)
        {
            return this.Return<int>(ApplicationName, RoleName, DeleteOnlyIfRoleIsEmpty);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
        public int aspnet_Roles_CreateRole(
            [MappedParameter(Size = 256)]string ApplicationName,
            [MappedParameter(Size = 256)]string RoleName)
        {
            return this.Return<int>(ApplicationName, RoleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod]
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