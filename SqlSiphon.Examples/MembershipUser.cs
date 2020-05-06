using System;

using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    [View(@"select
    a.ApplicationName,
    u.UserID,
    u.UserName,
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
from Membership m
inner join Users u on u.UserID = m.UserID
inner join Applications a on a.ApplicationID = m.ApplicationID")]
    public class MembershipUser
    {
        [Column(Size = 256)]
        public string ApplicationName { get; set; }

        public Guid UserID { get; set; }

        [Column(Size = 256)]
        public string UserName { get; set; }

        [Column(Size = 256)]
        public string Email { get; set; }

        [Column(Size = 256)]
        public string PasswordQuestion { get; set; }

        [Column(Size = 4)]
        public bool IsApproved { get; set; }

        [Column(Size = 4)]
        public bool IsLockedOut { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        public DateTime LastActivityDate { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }

        public DateTime LastLockoutDate { get; set; }

        [Column(Size = 256)]
        public string Comment { get; set; }
    }
}
