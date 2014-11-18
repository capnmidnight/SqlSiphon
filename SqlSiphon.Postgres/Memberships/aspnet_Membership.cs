﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Memberships
{
    [MappedClass]
    public class aspnet_Membership
    {
        [PK]
        public Guid UserId { get; set; }

        public Guid ApplicationId { get; set; }

        [MappedProperty(Size = 128)]
        public string Password { get; set; }

        [MappedProperty(DefaultValue = "0")]
        public int PasswordFormat { get; set; }

        [MappedProperty(Size = 128)]
        public string PasswordSalt { get; set; }

        [MappedProperty(Size = 16)]
        public string MobilePIN { get; set; }

        [MappedProperty(Size = 256, IsOptional = true)]
        public string Email { get; set; }

        [MappedProperty(Size = 256, IsOptional = true)]
        public string LoweredEmail { get; set; }

        [MappedProperty(Size = 256, IsOptional = true)]
        public string PasswordQuestion { get; set; }

        [MappedProperty(Size = 128, IsOptional = true)]
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
