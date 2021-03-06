﻿using System;

using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    [Table]
    public class Membership
    {
        [PK, FK(typeof(Users))]
        public Guid UserID { get; set; }

        [FK(typeof(Applications), AutoCreateIndex = false)]
        public Guid ApplicationID { get; set; }

        [Column(StringLength = 128)]
        public string Password { get; set; }

        [Column(DefaultValue = "0")]
        public int PasswordFormat { get; set; }

        [Column(StringLength = 128)]
        public string PasswordSalt { get; set; }

        [Column(StringLength = 16, IsOptional = true)]
        public string MobilePIN { get; set; }

        [Column(StringLength = 256, IsOptional = true)]
        public string Email { get; set; }

        [Column(StringLength = 256, IsOptional = true)]
        public string LoweredEmail { get; set; }

        [Column(StringLength = 256, IsOptional = true)]
        public string PasswordQuestion { get; set; }

        [Column(StringLength = 256, IsOptional = true)]
        public string PasswordAnswer { get; set; }

        [Column(DefaultValue = "false")]
        public virtual bool IsApproved { get; set; }

        [Column(DefaultValue = "false")]
        public virtual bool IsLockedOut { get; set; }

        [Column(DefaultValue = "getdate()")]
        public DateTime CreateDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }

        public DateTime LastLockoutDate { get; set; }

        public int FailedPasswordAttemptCount { get; set; }

        public DateTime FailedPasswordAttemptWindowStart { get; set; }

        public int FailedPasswordAnswerAttemptCount { get; set; }

        public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

        [Column(StringLength = 256, IsOptional = true)]
        public string Comment { get; set; }
    }
}
