using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    public class MembershipUser
    {
        [PK, FK(typeof(Users))]
        public Guid UserID { get; set; }

        [Column(Size = 256)]
        public string UserName { get; set; }

        [Column(Size = 256, IsOptional = true)]
        public string Email { get; set; }

        [Column(Size = 256, IsOptional = true)]
        public string PasswordQuestion { get; set; }

        public bool IsApproved { get; set; }

        public bool IsLockedOut { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        public DateTime LastActivityDate { get; set; }

        public DateTime LastPasswordChangedDate { get; set; }

        public DateTime LastLockoutDate { get; set; }

        [Column(Size = 256, IsOptional = true)]
        public string Comment { get; set; }
    }
}
