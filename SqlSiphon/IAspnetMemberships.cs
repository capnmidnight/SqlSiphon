using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public interface IAspnetMemberships : ISqlSiphon
    {
        //aspnet_Membership_ChangePasswordQuestionAndAnswer
        //aspnet_Membership_CreateUser
        //aspnet_Membership_FindUsersByEmail
        //aspnet_Membership_FindUsersByName
        //aspnet_Membership_GetAllUsers
        //aspnet_Membership_GetNumberOfUsersOnline
        //aspnet_Membership_GetPassword
        //aspnet_Membership_GetPasswordWithFormat
        //aspnet_Membership_GetUserByEmail
        //aspnet_Membership_GetUserByName
        //aspnet_Membership_GetUserByUserId
        //aspnet_Membership_ResetPassword
        //aspnet_Membership_SetPassword
        //aspnet_Membership_UnlockUser
        //aspnet_Membership_UpdateUser
        int aspnet_Membership_UpdateUserInfo(string ApplicationName, string UserName, bool IsPasswordCorrect, bool UpdateLastLoginActivityDate, int MaxInvalidPasswordAttempts, int PasswordAttemptWindow, DateTime CurrentTimeUtc, DateTime LastLoginDate, DateTime LastActivityDate);
        int aspnet_Roles_CreateRole(string ApplicationName, string RoleName);
        int aspnet_Roles_DeleteRole(string ApplicationName, string RoleName, bool DeleteOnlyIfRoleIsEmpty);
        List<string> aspnet_Roles_GetAllRoles(string ApplicationName);
        int aspnet_Roles_RoleExists(string ApplicationName, string RoleName);
        int aspnet_Users_CreateUser(Guid ApplicationId, string UserName, bool IsUserAnonymous, DateTime LastActivityDate, out Guid UserId);
        int aspnet_Users_DeleteUser(string ApplicationName, string UserName, int TablesToDeleteFrom, out int NumTablesDeletedFrom);
        int aspnet_UsersInRoles_AddUsersToRoles(string ApplicationName, string UserNames, string RoleNames, DateTime CurrentTimeUtc);
        List<string> aspnet_UsersInRoles_FindUsersInRole(string ApplicationName, string RoleName, string UserNameToMatch);
        List<string> aspnet_UsersInRoles_GetRolesForUser(string ApplicationName, string UserName);
        List<string> aspnet_UsersInRoles_GetUsersInRoles(string ApplicationName, string RoleName);
        int aspnet_UsersInRoles_IsUserInRole(string ApplicationName, string UserName, string RoleName);
        int aspnet_UsersInRoles_RemoveUsersFromRoles(string ApplicationName, string UserNames, string RoleNames);
    }
}
