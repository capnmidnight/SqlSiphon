using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer.Memberships
{
    [MappedClass]
    public class aspnet_Membership
    {
        public Guid UserId { get; set; }
        public Guid ApplicationId { get; set; }
        public string Password { get; set; }
        public int PasswordFormat { get; set; }
        public string PasswordSalt { get; set; }
        public string MobilePIN { get; set; }
        [MappedProperty(IsOptional = true)]
        public string Email { get; set; }
        [MappedProperty(IsOptional = true)]
        public string LoweredEmail { get; set; }
        [MappedProperty(IsOptional = true)]
        public string PasswordQuestion { get; set; }
        [MappedProperty(IsOptional = true)]
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public int FailedPasswordAttemptCount { get; set; }
        public DateTime FailedPasswordAttemptWindowStart { get; set; }
        public int FailedPasswordAnswerAttemptCount { get; set; }
        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }
        [MappedProperty(IsOptional = true)]
        public string Comment { get; set; }
    }
}
