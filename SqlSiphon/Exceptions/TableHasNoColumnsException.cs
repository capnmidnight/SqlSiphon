﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon
{
    public class PrimaryKeyColumnNotNullableException : Exception
    {
        public PrimaryKeyColumnNotNullableException(Mapping.TableAttribute table)
            : base(string.Format("The table `{0}`.`{1}` defined by type `{2}` has a primary key that includes a nullable column.", 
                table.Schema, 
                table.Name,
                ((Type)table.SourceObject).FullName))
        {
        }
    }
}
