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

using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.COLUMNS view.
    /// http://www.contrib.andrew.cmu.edu/~shadow/sql/sql1992.txt
    /// (text-search "CREATE VIEW COLUMNS")
    /// </summary>
    [MappedClass(Include = false, Schema = "information_schema")]
    public class Columns
    {
        public int? ordinal_position { get; set; }
        public int? character_maximum_length { get; set; }
        public int? character_octet_length { get; set; }
        public int? numeric_precision { get; set; }
        public int? numeric_precision_radix { get; set; }
        public int? numeric_scale { get; set; }
        public int? datetime_precision { get; set; }
        public string table_catalog { get; set; }
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string column_name { get; set; }
        public string column_default { get; set; }
        public string is_nullable { get; set; }
        public string data_type { get; set; }
        public string character_set_catalog { get; set; }
        public string character_set_schema { get; set; }
        public string character_set_name { get; set; }
        public string collation_catalog { get; set; }
        public string collation_schema { get; set; }
        public string collation_name { get; set; }
        public string domain_catalog { get; set; }
        public string domain_schema { get; set; }
        public string domain_name { get; set; }

        public int? interval_precision { get; set; }
        public int? maximum_cardinality { get; set; }
        public int? is_self_referencing { get; set; }
        public int? is_identity { get; set; }
        public int? is_generated { get; set; }
        public int? is_updatable { get; set; }
        public string identity_generation { get; set; }
        public int? identity_start { get; set; }
        public int? identity_increment { get; set; }
        public int? identity_maximum { get; set; }
        public int? identity_minimum { get; set; }
        public string identity_cycle { get; set; }
        public string interval_type { get; set; }
        public string udt_catalog { get; set; }
        public string udt_schema { get; set; }
        public string udt_name { get; set; }
        public string scope_catalog { get; set; }
        public string scope_schema { get; set; }
        public string scope_name { get; set; }
        public string dtd_identifier { get; set; }
        public string generation_expression { get; set; }
    }
}
