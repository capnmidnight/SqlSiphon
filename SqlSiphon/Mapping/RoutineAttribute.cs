using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// An attribute to use for tagging methods as being mapped to a stored procedure call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class RoutineAttribute : DatabaseObjectAttribute
    {
        /// <summary>
        /// The number of milliseconds before ADO.NET gives up waiting on the command.
        /// Defaults to -1, which specifies the default timeout for the database
        /// driver used by ADO.NET for the connection.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// The way that the command should be used. Either as a StoredProcedure
        /// or as a Text query. Defaults to StoredProcedure.
        /// </summary>
        public CommandType CommandType { get; set; }

        /// <summary>
        /// The script of the command. If this method should be mapped as a stored
        /// procedure, then it is used as part of a CREATE PROCEDURE script that
        /// is ran during a synchronization process. If the method is specified
        /// as a text query, then the query is ran directly.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// When set to true, instructs SqlSiphon to automatically wrap every
        /// query with a transaction. Defaults to false.
        /// </summary>
        public bool EnableTransaction { get; set; }

        /// <summary>
        /// A description of all the parameters that are part of the method.
        /// </summary>
        public List<ParameterAttribute> Parameters { get; private set; }

        /// <summary>
        /// Default constructor to set default values;
        /// </summary>
        public RoutineAttribute()
        {
            Parameters = new List<ParameterAttribute>();
            Timeout = -1;
            CommandType = CommandType.StoredProcedure;
            EnableTransaction = false;
        }

        public RoutineAttribute(InformationSchema.Routine routine, InformationSchema.Parameter[] parameters, ISqlSiphon dal)
        {
            Schema = routine.routine_schema;
            Name = routine.routine_name;
            CommandType = CommandType.StoredProcedure;
            Parameters = new List<ParameterAttribute>();

            if (routine.IsUserDefinedType)
            {
                SqlType = dal.MakeIdentifier(routine.type_udt_schema, routine.type_udt_name);
            }
            else
            {
                SqlType = dal.NormalizeSqlType(routine.data_type);
            }

            if (routine.is_array)
            {
                SqlType += "[]";
            }

            dal.AnalyzeQuery(routine.routine_definition, this);
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    Parameters.Add(new ParameterAttribute(p, dal));
                }
            }
        }

        /// <summary>
        /// Finds the ParameterAttribute for the giving ParameterInfo,
        /// or makes one up if it can't find one. All parameters in a mapped
        /// method will eventually have a ParameterAttribute represent it.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private ParameterAttribute ToColumn(ParameterInfo parameter)
        {
            return GetParameter(parameter) ?? new ParameterAttribute(parameter);
        }

        public static RoutineAttribute GetCommandDescription(MethodInfo method)
        {
            var meta = GetRoutine(method);
            if (meta != null && meta.CommandType == CommandType.TableDirect)
            {
                throw new NotImplementedException("Table-Direct queries are not supported by SqlSiphon");
            }

            return meta;
        }

        /// <summary>
        /// A virtual method to analyze an method and figure out the
        /// parameters and default settings for it. The attribute can't
        /// find the thing its attached to on its own, so this can't 
        /// be done in a constructor, we have to do it for it.
        /// 
        /// This method is not called from the DatabaseObjectAttribute.GetAttribute(s)
        /// methods because those methods aren't overloaded for different types
        /// of ICustomAttributeProvider types, but InferProperties is.
        /// </summary>
        /// <param name="obj">The method to InferProperties</param>
        protected override void InferProperties(MethodInfo obj)
        {
            base.InferProperties(obj);
            if (obj.ReturnType != typeof(void))
            {
                SystemType = obj.ReturnType;
            }
            Parameters.AddRange(obj.GetParameters()
                .Select(ToColumn));
        }

        public override string ToString()
        {
            return "ROUTINE " + base.ToString();
        }
    }
}
