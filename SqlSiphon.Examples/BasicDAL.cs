using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

using SqlSiphon.Mapping;


namespace SqlSiphon.Examples
{
    [InitializationScript(
@"insert into Applications
(ApplicationName, LoweredApplicationName, Description) values
('Test Application', 'test application', 'this is just a test');")]
    public class BasicDAL : DataConnector
    {
        public BasicDAL(IDataConnectorFactory factory, string server, string database, string userName, string password) :
            base(factory, server, database, userName, password)
        {
        }

        public BasicDAL(IDataConnector connection)
            : base(connection)
        {
        }

        public BasicDAL()
            : base()
        {
        }

        public static void FirstTimeSetup(BasicDAL db)
        {
            var appName = "TestApplication";
            var appID = db.GetApplicationID(appName);
            if (appID == Guid.Empty)
            {
                db.CreateApplication(appName, "an applciation for testing");
                appID = db.GetApplicationID(appName);
            }

            if (appID == Guid.Empty)
            {
                throw new Exception("Couldn't create the test application");
            }

            var roles = db.GetRoles(appID);
            var roleNames = roles.Select(r => r.LoweredRoleName).ToList();
            foreach (var roleName in new[] { "User", "Admin" })
            {
                if (!roleNames.Contains(roleName))
                {
                    db.CreateRole(roleName, appName, $"basic {roleName} role");
                }
            }
            roles = db.GetRoles(appID);

            var users = db.GetAllUsers(appName);
            foreach (var userName in new[] { "Anna", "Bob", "Christine", "Dave" })
            {
                var userID = db.GetUserID(userName);
                if (userID == Guid.Empty)
                {
                    userID = db.CreateUser(Guid.NewGuid(), userName, appName);
                    _ = db.CreateMembershipUser(
                        userID,
                        $"{userName}password",
                        "asdf12345",
                        $"{userName}@test.com",
                        "no question",
                        "no answer",
                        true,
                        DateTime.Now,
                        appName,
                        false);
                }

                var userRoles = db.GetRolesForUser(userName).ToList();
                if (!userRoles.Contains("User"))
                {
                    db.AddUserToRole(userName, "User");
                }
                if (userName == "Anna" && !userRoles.Contains("Admin"))
                {
                    db.AddUserToRole(userName, "Admin");
                }
            }
            users = db.GetAllUsers(appName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select ApplicationID 
into returnValue
from Applications
where ApplicationName = @applicationName;")]
        public Guid GetApplicationID(string applicationName)
        {
            return Get<Guid>(applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"insert into TestEntity1
(ColumnB, ColumnC, renamedColumnD, ColumnE) select
ColumnB, ColumnC, renamedColumnD, ColumnE
from @vals;")]
        public void UploadTestEntity(TestEntity1[] vals)
        {
            Execute(vals);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"insert into Applications 
(ApplicationID, ApplicationName, LoweredApplicationName, Description) values 
(newid(), @applicationName, lower(@applicationName), @description);")]
        public void CreateApplication(string applicationName, string description)
        {
            Execute(applicationName, description);
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select *
from MembershipUser
where userName = @userName
    and ApplicationName = @applicationName;")]
        public MembershipUser GetUserByUserName(string userName, string applicationName)
        {
            return Get<MembershipUser>(userName, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select userName
into returnValue
from MembershipUser
where lower(Email) = lower(@email)
    and ApplicationName = @applicationName;")]
        public string GetUserNameByEmail(string email, string applicationName)
        {
            return Get<string>(email, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select *
into returnValue
from MembershipUser
where UserID = @userID;")]
        public MembershipUser GetUserByID(Guid userID)
        {
            return Get<MembershipUser>(userID);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select UserID
into returnValue
from Users
where userName = @userName;")]
        public Guid GetUserID(string userName)
        {
            return Get<Guid>(userName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"insert into Users 
    (UserID,
    ApplicationID,
    userName,
    LoweredUserName,
    MobileAlias,
    IsAnonymous,
    LastActivityDate)
select
    @userID,
    ApplicationID,
    @userName,
    lower(@userName),
    NULL,
    @isAnonymous,
    GetDate()
from Applications
where ApplicationName = @applicationName;")]
        public void InsertUser(Guid userID, string userName, string applicationName, bool isAnonymous)
        {
            Execute(userID, userName, applicationName, isAnonymous);
        }
        public void InsertUser(Guid userID, string userName, string applicationName)
        {
            InsertUser(userID, userName, applicationName, false);
        }

        public Guid CreateUser(Guid userID, string userName, string applicationName)
        {
            InsertUser(userID, userName, applicationName);
            return GetUserID(userName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"insert into Membership
    (UserID,
    ApplicationID,
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
select
    @userID,
    ApplicationID,
    @password, 
    @passwordSalt,
    @email, 
    lower(@email),
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
    @creationDate
from Applications
where ApplicationName = @applicationName;")]
        public void InsertMembershipUser(Guid userID, string password, string passwordSalt, string email, string passwordQuestion, string passwordAnswer, bool isApproved, DateTime creationDate, string applicationName, bool isLockedOut)
        {
            Execute(userID, password, passwordSalt, email, passwordQuestion, passwordAnswer, isApproved, creationDate, applicationName, isLockedOut);
        }

        public MembershipUser CreateMembershipUser(Guid userID, string password, string passwordSalt, string email, string passwordQuestion, string passwordAnswer, bool isApproved, DateTime creationDate, string applicationName, bool isLockedOut)
        {
            InsertMembershipUser(userID, password, passwordSalt, email, passwordQuestion, passwordAnswer, isApproved, creationDate, applicationName, isLockedOut);
            return GetUserByID(userID);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select @userID = UserID
from Users
where userName = @userName
    and ApplicationID = @applicationID;

delete from Membership where UserID = @UserID
    and ApplicationID = @applicationID;

delete from Users where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void DeleteUser(string userName, string applicationName)
        {
            Execute(userName, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select @userID = UserID
from Users
where userName = @userName
    and ApplicationID = @applicationID;

update Membership
set Email = @email,
    LoweredEmail = lower( @email),
    Comment = @comment,
    IsApproved = @isApproved
where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void UpdateUser(string userName, string applicationName, string email, string comment, bool isApproved)
        {
            Execute(userName, applicationName, email, comment, isApproved);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"return query select *
from MembershipUser
where ApplicationName = @applicationName
order by userName asc;")]
        public List<MembershipUser> GetAllUsers(string applicationName)
        {
            return GetList<MembershipUser>(applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select Count(*)
into returnValue
from Users
where LastActivityDate > @compareDate
    and ApplicationID = @applicationID;")]
        public int GetNumberOfUsersOnline(DateTime compareDate, string applicationName)
        {
            return Get<int>(compareDate, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select 
    u.UserID, 
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
from Membership m
inner join Users u on u.UserID = m.UserID
inner join Applications a on a.ApplicationID = m.ApplicationID
where u.userName = @userName
    and a.ApplicationName = @applicationName;")]
        public Membership GetMembership(string userName, string applicationName)
        {
            return Get<Membership>(userName, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select @userID = UserID
from Users
where userName = @userName
    and ApplicationID = @applicationID;

update Membership set
    Password = @newPassword,
    LastPasswordChangedDate = @lastPasswordChangedDate,
    ApplicationID = @applicationID
where UserID = @userID;")]
        public void ChangePassword(string userName, string newPassword, DateTime lastPasswordChangedDate, string applicationName)
        {
            Execute(userName, newPassword, lastPasswordChangedDate, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

update Membership set
    LastLoginDate = @lastLoginDate
where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void UserLogin(DateTime lastLoginDate, Guid userID, string applicationName)
        {
            Execute(lastLoginDate, userID, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select @userID = UserID
from Users
where userName = @userName
    and ApplicationID = @applicationID;

        
update Membership
set FailedPasswordAttemptCount  = @failedPasswordAttemptCount,
    FailedPasswordAttemptWindowStart  = @failedPasswordAttemptWindowStart 
where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void UpdateFailedPasswordAttemptCountAndDate(string userName, string applicationName, int failedPasswordAttemptCount, DateTime failedPasswordAttemptWindowStart)
        {
            Execute(userName, applicationName, failedPasswordAttemptCount, failedPasswordAttemptWindowStart);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select @userID = UserID
from Users
where userName = @userName
    and ApplicationID = @applicationID;

update Membership
set FailedPasswordAttemptCount  = @failedPasswordAttemptCount
where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void UpdateFailedPasswordAttemptCount(string userName, string applicationName, int failedPasswordAttemptCount)
        {
            Execute(userName, applicationName, failedPasswordAttemptCount);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select @userID = UserID
from Users
where userName = @userName
    and ApplicationID = @applicationID;

update Membership
set FailedPasswordAnswerAttemptCount   = @failedPasswordAnswerAttemptCount,
    FailedPasswordAnswerAttemptWindowStart = @failedPasswordAnswerAttemptWindowStart 
where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void UpdateFailedPasswordAnswerAttemptCountAndDate(string userName, string applicationName, int failedPasswordAnswerAttemptCount, DateTime failedPasswordAnswerAttemptWindowStart)
        {
            Execute(userName, applicationName, failedPasswordAnswerAttemptCount, failedPasswordAnswerAttemptWindowStart);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select @userID = UserID
from Users
where userName = @userName
    and ApplicationID = @applicationID;

update Membership
set FailedPasswordAnswerAttemptCount = @failedPasswordAnswerAttemptCount
where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void UpdateFailedPasswordAnswerAttemptCount(string userName, string applicationName, int failedPasswordAnswerAttemptCount)
        {
            Execute(userName, applicationName, failedPasswordAnswerAttemptCount);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

select @userID = UserID
from Users
where userName = @userName
    and ApplicationID = @applicationID;

update Membership
set IsLockedOut = @isLockedOut,
    LastLockoutDate = @lastLockoutDate
where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void LockUnlockUser(string userName, string applicationName, bool isLockedOut, DateTime lastLockoutDate)
        {
            Execute(userName, applicationName, isLockedOut, lastLockoutDate);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"return query select *
from MembershipUser
where userName LIKE @userNameToMatch
    and ApplicationName = @applicationName;")]
        public List<MembershipUser> FindUsersByName(string userNameToMatch, string applicationName)
        {
            return GetList<MembershipUser>(userNameToMatch, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"return query select *
from MembershipUser
where lower(Email) like lower(@emailToMatch)
    and ApplicationName = @applicationName;")]
        public List<MembershipUser> FindUsersByEmail(string emailToMatch, string applicationName)
        {
            return GetList<MembershipUser>(emailToMatch, applicationName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;
declare @userID uniqueidentifier;

select @userID = UserID
from Users
where userName = @userName;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

update Membership
set PasswordQuestion = @question,
    PasswordAnswer = @answer
where UserID = @userID
    and ApplicationID = @applicationID;")]
        public void ChangePasswordQuestionAndAnswer(string userName, string applicationName, string question, string answer)
        {
            Execute(userName, applicationName, question, answer);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"update Membership set 
    Email = @newAddress,
    LoweredEmail = lower(@newAddress)
where UserID = @userID;")]
        public void ChangeEmail(Guid userID, string newAddress)
        {
            Execute(userID, newAddress);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @roleID uniqueidentifier;
declare @userID uniqueidentifier;

select @roleID = RoleID
from Roles
where RoleName = @roleName;

select @userID = UserID
from Users
where userName = @userName;

insert into UsersInRoles
(UserID, RoleID) Values 
(@userID, @roleID);")]
        public void AddUserToRole(string userName, string roleName)
        {
            Execute(userName, roleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @applicationID uniqueidentifier;

select @applicationID = ApplicationID
from Applications
where ApplicationName = @applicationName;

insert into Roles 
(RoleID, ApplicationID, RoleName, LoweredRoleName, Description) Values 
(newid(), @applicationID, @roleName, lower(@roleName), @description);")]
        public void CreateRole(string roleName, string applicationName, string description)
        {
            Execute(roleName, applicationName, description);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"delete from UsersInRoles
	where RoleID = (select RoleID from Roles where RoleName = @roleName);")]
        public void DeleteUsersInRole(string roleName)
        {
            Execute(roleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"delete from Roles
where RoleName = @roleName;")]
        public void DeleteRole(string roleName)
        {
            // remove FK items
            DeleteUsersInRole(roleName);
            Execute(roleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"return query select RoleName 
from Roles;")]
        public string[] GetAllRoles()
        {
            return GetList<string>().ToArray();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"return query select 
    RoleID,
    ApplicationID,
    RoleName,
    LoweredRoleName,
    Description
from Roles
where ApplicationID = @applicationID;")]
        public List<Roles> GetRoles(Guid applicationID)
        {
            return GetList<Roles>(applicationID);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"return query select Roles.RoleName
from UsersInRoles
	inner join Users on Users.UserID = UsersInRoles.UserID
	inner join Roles on Roles.RoleID = UsersInRoles.RoleID
where Users.userName = @userName;", StringLength = 256)]
        public string[] GetRolesForUser(string userName)
        {
            return GetList<string>(userName).ToArray();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"return query select Users.userName 
from UsersInRoles
	inner join Users on Users.UserID = UsersInRoles.UserID
	inner join Roles on Roles.RoleID = UsersInRoles.RoleID
where Roles.RoleName = @roleName;")]
        public string[] GetUsersInRole(string roleName)
        {
            return GetList<string>(roleName).ToArray();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select COUNT(*)
into returnValue
from UsersInRoles
    inner join Users on Users.UserID = UsersInRoles.UserID
	inner join Roles on Roles.RoleID = UsersInRoles.RoleID
where Users.userName= @userName
    and Roles.RoleName = @roleName;")]
        public bool IsUserInRole(string userName, string roleName)
        {
            return Get<int>(userName, roleName) > 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"declare @UserID uniqueidentifier;
declare @RoleID uniqueidentifier;

select @UserID = UserID
from Users
where userName = @userName;

select @RoleID = RoleID
from Roles
where RoleName = @roleName;

delete from UsersInRoles
where UserID = @UserID
    and RoleID = @RoleID;")]
        public void RemoveUserFromRole(string userName, string roleName)
        {
            Execute(userName, roleName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"select COUNT(*)
into returnValue
from Roles
where RoleName = @roleName;")]
        public bool RoleExists(string roleName)
        {
            return Get<int>(roleName) > 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@"return query select Users.userName 
from UsersInRoles
    inner join Users on Users.UserID = UsersInRoles.UserID
    inner join Roles on Roles.RoleID = UsersInRoles.RoleID
where Users.userName like @userNameToMatch
    and Roles.RoleName = @roleName;")]
        public string[] FindUsersInRole(string userNameToMatch, string roleName)
        {
            return GetList<string>(userNameToMatch, roleName).ToArray();
        }
    }
}
