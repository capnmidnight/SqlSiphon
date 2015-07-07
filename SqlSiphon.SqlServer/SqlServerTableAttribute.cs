﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.SqlServer
{
    /// <summary>
    /// An attribute to use for tagging methods as being mapped to a stored procedure call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public class SqlServerTableAttribute : SqlSiphon.Mapping.TableAttribute
    {
        public bool IsUploadable { get; set; }

        public SqlServerTableAttribute() { }

        public SqlServerTableAttribute(Type t)
        {
            this.InferProperties(t);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public class UploadableAttribute : SqlServerTableAttribute
    {
        public UploadableAttribute()
        {
            this.IsUploadable = true;
            this.Include = false;
        }

        public UploadableAttribute(Type t)
            : base(t)
        {
            this.IsUploadable = true;
            this.Include = false;
        }
    }
}
