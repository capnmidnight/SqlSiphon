using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public interface IAspnetMemberships : ISqlSiphon
    {
//        aspnet_Setup_RestorePermissions
//aspnet_Setup_RemoveAllRoleMembers
//aspnet_RegisterSchemaVersion
//aspnet_CheckSchemaVersion
//aspnet_Applications_CreateApplication
//aspnet_UnRegisterSchemaVersion
//aspnet_Users_CreateUser
//aspnet_Users_DeleteUser
//aspnet_AnyDataInTables
//aspnet_Membership_CreateUser
//aspnet_Membership_GetUserByName
//aspnet_Membership_GetUserByUserId
//aspnet_Membership_GetUserByEmail
//aspnet_Membership_GetPasswordWithFormat
//aspnet_Membership_UpdateUserInfo
//aspnet_Membership_GetPassword
//aspnet_Membership_SetPassword
//aspnet_Membership_ResetPassword
//aspnet_Membership_UnlockUser
//aspnet_Membership_UpdateUser
//aspnet_Membership_ChangePasswordQuestionAndAnswer
//aspnet_Membership_GetAllUsers
//aspnet_Membership_GetNumberOfUsersOnline
//aspnet_Membership_FindUsersByName
//aspnet_Membership_FindUsersByEmail
//aspnet_UsersInRoles_IsUserInRole
//aspnet_UsersInRoles_GetRolesForUser
//aspnet_Roles_CreateRole
//aspnet_Roles_DeleteRole
//aspnet_Roles_RoleExists
//aspnet_UsersInRoles_AddUsersToRoles
        int aspnet_UsersInRoles_RemoveUsersFromRoles(string ApplicationName, string UserNames, string RoleNames);
//aspnet_UsersInRoles_GetUsersInRoles
//aspnet_UsersInRoles_FindUsersInRole
//aspnet_Roles_GetAllRoles
    }
    /// <summary>
    /// A simple mixin for any implementing user wanting to use ASP.NET Memberships.
    /// </summary>
    public static class IAspnetMemebershipsExt
    {
        public static string FKToUsers<T>(this IAspnetMemberships dal)
        {
            return dal.FKToUsers<T>("UserID");
        }

        public static string FKToUsers<T>(this IAspnetMemberships dal, string tableColumn)
        {
            return dal.FK<T>(tableColumn, null, "aspnet_Users", "UserID");
        }

        public static string FKToRoles<T>(this IAspnetMemberships dal)
        {
            return dal.FKToRoles<T>("RoleID");
        }

        public static string FKToRoles<T>(this IAspnetMemberships dal, string tableColumn)
        {
            return dal.FK<T>(tableColumn, null, "aspnet_Roles", "RoleID");
        }
    }
}
