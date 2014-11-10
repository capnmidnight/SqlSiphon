using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public interface IAspnetMemberships : ISqlSiphon
    {

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
            return dal.FK<T>(tableColumn, "dbo", "aspnet_Users", "UserID");
        }

        public static string FKToRoles<T>(this IAspnetMemberships dal)
        {
            return dal.FKToRoles<T>("RoleID");
        }

        public static string FKToRoles<T>(this IAspnetMemberships dal, string tableColumn)
        {
            return dal.FK<T>(tableColumn, "dbo", "aspnet_Roles", "RoleID");
        }
    }
}
