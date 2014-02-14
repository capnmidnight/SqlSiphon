/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009 - 2014 Sean T. McBeth
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this 
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or 
  other materials provided with the distribution.

* Neither the name of McBeth Software Systems nor the names of its contributors
  may be used to endorse or promote products derived from this software without 
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Reflection;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// An attribute to tag properties in a class, for optional
    /// information about how the properties maps to a column 
    /// in a table.
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class MappedPropertyAttribute : MappedTypeAttribute
    {
        private PropertyInfo originalProperty;
        /// <summary>
        /// Gets or sets whether or not this column holds an
        /// auto-incrementing integer value. Defaults to false.
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Get or set a value indicating that the column
        /// is used as part of the table's primary key.
        /// Defaults to false.
        /// </summary>
        public bool IncludeInPrimaryKey { get; set; }

        /// <summary>
        /// When a property references another mapped class,
        /// it will be used to specify a foreign key relation-
        /// ship. The mapped table for this class will have
        /// to have columns added to it that match the
        /// primary key of the foreign table. By default,
        /// those columns will be named the same name as they
        /// are named in the foreign table. This will cause
        /// a conflict in the case of one table having foreign-
        /// key references to any set of tables whose primary
        /// keys are named the same. By setting the PrefixColumnNames
        /// property to true, the columns added to this table
        /// will have the name of the property this attribute
        /// is tagged to used as a prefix on each column's name.
        /// Defaults to true.
        /// </summary>
        public bool PrefixColumnNames { get; set; }

        /// <summary>
        /// If the property this attribute is tagged to specifies
        /// a foreign key relationship, then setting the Cascade
        /// property will specify CASCADE DELETE and CASCADE UPDATE
        /// on the foreign key constraint. Defaults to true.
        /// </summary>
        public bool Cascade { get; set; }

        /// <summary>
        /// Specifies the property maps to a column in a table that
        /// is not an auto-incrementing integer field and is not 
        /// included in the primary key. If the column is part of a
        /// foreign key relationship, it cascades deletes and updates
        /// and is named based both on the property name and the 
        /// foreign table's primary key column names. These default
        /// settings can be overridden.
        /// </summary>
        public MappedPropertyAttribute()
        {
            this.IsIdentity = false;
            this.IncludeInPrimaryKey = false;
            this.PrefixColumnNames = true;
            this.Cascade = true;
        }

        public override void InferProperties(System.Reflection.PropertyInfo obj)
        {
            base.InferProperties(obj);
            this.originalProperty = obj;
        }

        public void SetValue(object obj, object value)
        {
            if (value == DBNull.Value)
                value = null;
            this.originalProperty.SetValue(obj, value, null);
        }

        public T GetValue<T>(object obj)
        {
            return (T)this.originalProperty.GetValue(obj, null);
        }
    }
}
