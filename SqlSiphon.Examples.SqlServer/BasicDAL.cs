using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.CompilerServices;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.SqlServer;

namespace SqlSiphon.Examples.SqlServer
{
    public class BasicDAL : SqlServerDataAccessLayer
    {
        public BasicDAL(string connectionString)
            : base(connectionString)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"INSERT INTO Applications (ApplicationId, ApplicationName, LoweredApplicationName, Description)
	Values (NewID(), @applicationName, LOWER(@applicationName), @description);")]
        public void CreateApplication(string applicationName, string description)
        {
            this.Execute(applicationName, description);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select 
    U.UserID, 
    U.UserName,
    M.Email,
    R.RoleID,
    R.RoleName,
    RT.RoleTypeID,
    RT.RoleTypeName
from Users U
    inner join Membership M on M.UserId = U.UserId 
    left outer join UsersInRoles UR on U.UserID = UR.UserID
    left outer join Roles R on UR.RoleID = R.RoleID
    left outer join RoleTypesForRoles RTR on R.RoleID = RTR.RoleID
    left outer join RoleTypes RT on RTR.RoleTypeID = RT.RoleTypeID
union
select 
    U.UserID, 
    U.UserName,
    M.Email,
    R.RoleID,
    R.RoleName,
    RT.RoleTypeID,
    RT.RoleTypeName
from Users U
    join Membership M on U.UserID = M.UserID
    join UsersInRoles UR on U.UserID = UR.UserID
    join RoleHierarchy RH on UR.RoleId = RH.RoleID
    join Roles R on R.RoleID = RH.ParentRoleID
    join RoleTypesForRoles RTR on R.RoleID = RTR.RoleID
    join RoleTypes RT on RTR.RoleTypeID = RT.RoleTypeID;")]
        public List<UserWithRole> GetUsersWithRoles()
        {
            return this.GetList<UserWithRole>();
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
SELECT 
    u.UserId, 
    u.Username, 
    m.Email, 
    m.PasswordQuestion,
    m.Comment, 
    m.IsApproved, 
    m.IsLockedOut, 
    m.CreateDate, 
    m.LastLoginDate,
    u.LastActivityDate, 
    m.LastPasswordChangedDate, 
    m.LastLockoutDate
FROM Membership m
INNER JOIN Users u on u.UserId = m.UserId
INNER JOIN Applications a on a.ApplicationId = m.ApplicationId
WHERE m.UserId = @userId and m.ApplicationId = @applicationId;")]
        public MembershipUser GetUserByUserName(string username, string applicationName)
        {
            return this.Get<MembershipUser>(username, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
SELECT 
    u.Username
FROM Users u
INNER JOIN Membership m on m.UserId = u.UserId
WHERE m.LoweredEmail = @email and u.ApplicationId = @applicationId;")]
        public string GetUserNameByEmail(string email, string applicationName)
        {
            return this.Get<string>("Username", email.ToLower(), applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"SELECT 
    u.UserId, 
    u.Username, 
    m.Email, 
    m.PasswordQuestion,
    m.Comment, 
    m.IsApproved, 
    m.IsLockedOut, 
    m.CreateDate, 
    m.LastLoginDate,
    u.LastActivityDate, 
    m.LastPasswordChangedDate, 
    m.LastLockoutDate
FROM Membership m
INNER JOIN Users u on u.UserId = m.UserId
WHERE m.UserId = @userId;")]
        public MembershipUser GetUserById(Guid userId)
        {
            return this.Get<MembershipUser>(userId);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"SELECT UserId
   FROM Users
    WHERE UserName = @username;")]
        public Guid GetUserId(string username)
        {
            return this.Get<Guid>("UserId", username);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
INSERT INTO Users 
    (UserId,
    ApplicationId,
    UserName,
    LoweredUserName,
    MobileAlias,
    IsAnonymous,
    LastActivityDate)
VALUES
    (@userId,
    @applicationId,
    @username,
    LOWER(@username),
    NULL,
    0,
    GetDate());")]
        public Guid CreateUser(Guid userId,
                                        string username,
                                        string applicationName)
        {
            this.Execute(userId,
                        username,
                        applicationName);

            return this.GetUserId(username);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
INSERT INTO Membership
    (UserId,
    ApplicationId,
    Password, 
    PasswordSalt,
    Email, 
    LoweredEmail,
    PasswordQuestion,
    PasswordAnswer, 
    PasswordFormat,
    IsApproved, 
    IsLockedOut,
    Comment, 
    CreateDate, 
    LastPasswordChangedDate, 
    LastLoginDate,
    LastLockoutDate,
    FailedPasswordAttemptCount, 
    FailedPasswordAttemptWindowStart,
    FailedPasswordAnswerAttemptCount, 
    FailedPasswordAnswerAttemptWindowStart)
values (
    @userId,
    @applicationId,
    @password, 
    @passwordSalt,
    @email, 
    LOWER(@email),
    @passwordQuestion, 
    @passwordAnswer, 
    0,
    @isApproved, 
    @isLockedOut, 
    '', 
    @creationDate, 
    @creationDate, 
    @creationDate,
    @creationDate,
    0,
    @creationDate,
    0,
    @creationDate);")]
        public MembershipUser CreateMembershipUser(Guid userId,
                                string password,
                                string passwordSalt,
                                string email,
                                string passwordQuestion,
                                string passwordAnswer,
                                bool isApproved,
                                DateTime creationDate,
                                string applicationName,
                                bool isLockedOut)
        {
            //DateTime creationDate = DateTime.Now;
            //string comments = string.Empty;
            //bool isLockedOut = false;

            this.Execute(userId,
                        password,
                        passwordSalt,
                        email,
                        passwordQuestion,
                        passwordAnswer,
                        isApproved,
                        creationDate,
                        applicationName,
                        isLockedOut);

            return this.GetUserById(userId);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
declare @userId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
delete from Membership where UserId = @UserId and ApplicationId = @applicationId;
delete from Users where UserID = @userId and ApplicationId = @applicationId;")]
        public void DeleteUser(string username, string applicationName)
        {
            this.Execute(username, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
Update Membership
SET Email = @email,
    LoweredEmail = LOWER( @email),
    Comment = @comment,
    IsApproved = @isApproved
WHERE UserId = @userId AND ApplicationId = @applicationId;")]
        public void UpdateUser(string username, string applicationName, string email, string comment, bool isApproved)
        {
            this.Execute(username, applicationName, email, comment, isApproved);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
SELECT 
    u.UserId, 
    u.Username, 
    m.Email, 
    m.PasswordQuestion,
    m.Comment, 
    m.IsApproved, 
    m.IsLockedOut, 
    m.CreateDate, 
    m.LastLoginDate,
    u.LastActivityDate, 
    m.LastPasswordChangedDate, 
    m.LastLockoutDate
FROM Membership m
INNER JOIN Users u on u.UserId = m.UserId
WHERE m.ApplicationId = @applicationId
 ORDER BY Username Asc;")]
        public List<MembershipUser> GetAllUsers(string applicationName)
        {
            return this.GetList<MembershipUser>(applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
SELECT Count(*) FROM Users
WHERE LastActivityDate > @compareDate AND ApplicationId = @applicationId;")]
        public int GetNumberOfUsersOnline(DateTime compareDate, string applicationName)
        {
            return this.Get<int>(compareDate, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
SELECT 
    u.UserId, 
    a.ApplicationID, 
    m.Password,
    m.PasswordFormat,
    m.PasswordSalt,
    m.MobilePIN,
    m.Email, 
    m.LoweredEmail,
    m.PasswordQuestion,
    m.PasswordAnswer,
    m.IsApproved, 
    m.IsLockedOut, 
    m.CreateDate, 
    m.LastLoginDate,
    m.LastPasswordChangedDate, 
    m.LastLockoutDate,
    m.FailedPasswordAttemptCount,
    m.FailedPasswordAttemptWindowStart,
    m.FailedPasswordAnswerAttemptCount,
    m.FailedPasswordAnswerAttemptWindowStart,
    m.Comment
FROM Membership m
INNER JOIN Users u on u.UserId = m.UserId
INNER JOIN Applications a on a.ApplicationID = m.ApplicationID
WHERE u.UserId = @userId AND a.ApplicationId = @applicationId;")]
        public Membership GetMembership(string username, string applicationName)
        {
            return this.Get<Membership>(username, applicationName);
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
UPDATE Membership
SET Password = @newPassword,
LastPasswordChangedDate  = @lastPasswordChangedDate,
ApplicationId = @applicationId
WHERE UserId = @userId;")]
        public void ChangePassword(string username, string newPassword, DateTime lastPasswordChangedDate, string applicationName)
        {
            this.Execute(username, newPassword, lastPasswordChangedDate, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
UPDATE Membership
SET LastLoginDate = @lastLoginDate
WHERE UserId = @userId AND ApplicationId = @applicationId;")]
        public void UserLogin(DateTime lastLoginDate, Guid userId, string applicationName)
        {
            this.Execute(lastLoginDate, userId, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
        
        UPDATE Membership
SET FailedPasswordAttemptCount  = @failedPasswordAttemptCount,
    FailedPasswordAttemptWindowStart  = @failedPasswordAttemptWindowStart 
WHERE UserId = @userId AND ApplicationId = @applicationId;")]
        public void UpdateFailedPasswordAttemptCountAndDate(string username, string applicationName, int failedPasswordAttemptCount, DateTime failedPasswordAttemptWindowStart)
        {
            this.Execute(username, applicationName, failedPasswordAttemptCount, failedPasswordAttemptWindowStart);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
UPDATE Membership
SET FailedPasswordAttemptCount  = @failedPasswordAttemptCount
WHERE UserId = @userId AND ApplicationId = @applicationId;")]
        public void UpdateFailedPasswordAttemptCount(string username, string applicationName, int failedPasswordAttemptCount)
        {
            this.Execute(username, applicationName, failedPasswordAttemptCount);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
UPDATE Membership
SET FailedPasswordAnswerAttemptCount   = @failedPasswordAnswerAttemptCount,
    FailedPasswordAnswerAttemptWindowStart = @failedPasswordAnswerAttemptWindowStart 
WHERE UserId = @userId AND ApplicationId = @applicationId;")]
        public void UpdateFailedPasswordAnswerAttemptCountAndDate(string username, string applicationName, int failedPasswordAnswerAttemptCount, DateTime failedPasswordAnswerAttemptWindowStart)
        {
            this.Execute(username, applicationName, failedPasswordAnswerAttemptCount, failedPasswordAnswerAttemptWindowStart);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
UPDATE Membership
SET FailedPasswordAnswerAttemptCount   = @failedPasswordAnswerAttemptCount
WHERE UserId = @userId AND ApplicationId = @applicationId;")]
        public void UpdateFailedPasswordAnswerAttemptCount(string username, string applicationName, int failedPasswordAnswerAttemptCount)
        {
            this.Execute(username, applicationName, failedPasswordAnswerAttemptCount);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
declare @userId uniqueidentifier;
select @userId = UserId from Users where UserName = @username AND ApplicationId = @applicationId;
UPDATE Membership
SET IsLockedOut = @isLockedOut,
    LastLockoutDate = @lastLockoutDate
WHERE UserId = @userId AND ApplicationId = @applicationId;")]
        public void LockUnlockUser(string username, string applicationName, bool isLockedOut, DateTime lastLockoutDate)
        {
            this.Execute(username, applicationName, isLockedOut, lastLockoutDate);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
SELECT 
    u.UserId, 
    u.Username, 
    m.Email, 
    m.PasswordQuestion,
    m.Comment, 
    m.IsApproved, 
    m.IsLockedOut, 
    m.CreateDate, 
    m.LastLoginDate,
    u.LastActivityDate, 
    m.LastPasswordChangedDate, 
    m.LastLockoutDate
FROM Membership m
INNER JOIN Users u on u.UserId = m.UserId
INNER JOIN Applications a on a.ApplicationId = m.ApplicationId
WHERE u.Username LIKE @usernameToMatch AND m.ApplicationId = @applicationId;")]
        public List<MembershipUser> FindUsersByName(string usernameToMatch, string applicationName)
        {
            return this.GetList<MembershipUser>(usernameToMatch, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
SELECT 
    u.UserId, 
    u.Username, 
    m.Email, 
    m.PasswordQuestion,
    m.Comment, 
    m.IsApproved, 
    m.IsLockedOut, 
    m.CreateDate, 
    m.LastLoginDate,
    u.LastActivityDate, 
    m.LastPasswordChangedDate, 
    m.LastLockoutDate
FROM Membership m
INNER JOIN Users u on u.UserId = m.UserId
INNER JOIN Applications a on a.ApplicationId = m.ApplicationId
WHERE Email LIKE @emailToMatch AND m.ApplicationId = @applicationId;")]
        public List<MembershipUser> FindUsersByEmail(string emailToMatch, string applicationName)
        {
            return this.GetList<MembershipUser>(emailToMatch, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"
declare @userId uniqueidentifier;
select @userId = UserID from Users where UserName = @username;
declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
UPDATE Membership
SET PasswordQuestion = @question,
    PasswordAnswer = @answer
WHERE UserId = @userId AND ApplicationId = @applicationId;")]
        public void ChangePasswordQuestionAndAnswer(string username, string applicationName, string question, string answer)
        {
            this.Execute(username, applicationName, question, answer);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"update Membership
    set Email = @newAddress,
        LoweredEmail = LOWER(@newAddress)
where UserID = @userID;")]
        public void ChangeEmail(Guid userID, string newAddress)
        {
            this.Execute(userID, newAddress);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"INSERT INTO UsersInRoles (UserId, RoleId)
	Values ((select UserId from Users where UserName = @userName),
            (select RoleId from Roles where RoleName = @rolename));")]
        public void AddUserToRole(string username, string rolename)
        {
            this.Execute(username, rolename);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationId uniqueidentifier;
select @applicationId = ApplicationId from Applications where ApplicationName = @applicationName;
INSERT INTO Roles (RoleId, ApplicationId, Rolename, LoweredRoleName, Description)
	Values (NewID(), @applicationId, @rolename, LOWER(@rolename), @description);")]
        public void CreateRole(string rolename, string applicationName, string description)
        {
            this.Execute(rolename, applicationName, description);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"DELETE FROM UsersInRoles
	WHERE RoleId = (select RoleId from Roles where RoleName = @rolename);")]
        public void DeleteUsersInRole(string rolename)
        {
            this.Execute(rolename);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"DELETE FROM RoleTypesForRoles
	WHERE RoleId = (select RoleId from Roles where RoleName = @rolename);")]
        public void DeleteRoleTypesForRoles(string rolename)
        {
            this.Execute(rolename);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"DELETE FROM RoleHierarchy
	WHERE RoleId = (select RoleId from Roles where RoleName = @rolename)
    OR ParentRoleId = (select RoleId from Roles where RoleName = @rolename);")]
        public void DeleteRoleHierarchy(string rolename)
        {
            this.Execute(rolename);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"DELETE FROM Roles
	WHERE RoleName = @rolename;")]
        public void DeleteRole(string rolename)
        {
            // remove FK items
            this.DeleteUsersInRole(rolename);
            this.DeleteRoleTypesForRoles(rolename);
            this.DeleteRoleHierarchy(rolename);

            this.Execute(rolename);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"SELECT RoleName FROM Roles;")]
        public string[] GetAllRoles()
        {
            return this.GetList<string>("RoleName").ToArray();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"SELECT Roles.Rolename FROM UsersInRoles
	INNER JOIN Users on Users.UserId = UsersInRoles.UserId
	INNER JOIN Roles on Roles.RoleId = UsersInRoles.RoleId
WHERE Users.UserName= @username;")]
        public string[] GetRolesForUser(string username)
        {
            return this.GetList<string>("RoleName", username).ToArray();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"SELECT Users.UserName 
FROM UsersInRoles
	INNER JOIN Users on Users.UserId = UsersInRoles.UserId
	INNER JOIN Roles on Roles.RoleId = UsersInRoles.RoleId
WHERE Roles.RoleName = @rolename;")]
        public string[] GetUsersInRole(string rolename)
        {
            return this.GetList<string>("UserName", rolename).ToArray();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"SELECT COUNT(*) 
FROM UsersInRoles
INNER JOIN Users on Users.UserId = UsersInRoles.UserId
	INNER JOIN Roles on Roles.RoleId = UsersInRoles.RoleId
WHERE Users.UserName= @username AND Roles.RoleName = @rolename;")]
        public bool IsUserInRole(string username, string rolename)
        {
            return this.Get<int>(0, username, rolename) > 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @UserID uniqueidentifier;
declare @RoleID uniqueidentifier;
select @UserID = UserID from Users where UserName = @username;
select @RoleID = RoleID from Roles where RoleName = @rolename;
delete from UsersInRoles where UserID = @UserID and RoleID = @RoleID;")]
        public void RemoveUserFromRole(string username, string rolename)
        {
            this.Execute(username, rolename);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"SELECT COUNT(*) FROM Roles
	WHERE RoleName = @rolename;")]
        public bool RoleExists(string rolename)
        {
            return this.Get<int>(0, rolename) > 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"SELECT Users.Username FROM UsersInRoles
	INNER JOIN Users on Users.UserId = UsersInRoles.UserId
	INNER JOIN Roles on Roles.RoleId = UsersInRoles.RoleId
	WHERE Users.UserName like @userNameToMatch AND Roles.RoleName = @rolename;")]
        public string[] FindUsersInRole(string userNameToMatch, string rolename)
        {
            return this.GetList<string>("Username", userNameToMatch, rolename).ToArray();
        }
    }
}
