using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer
{
    public class UserWithRole
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public Guid? RoleID { get; set; }
        public string RoleName { get; set; }

        public int? RoleTypeID { get; set; }
        public string RoleTypeName { get; set; }
    }
}
