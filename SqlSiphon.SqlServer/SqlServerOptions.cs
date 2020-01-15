using System;

namespace SqlSiphon.SqlServer
{
    /// <summary>
    /// A flags enumeration for specifying the options that are set ON or OFF
    /// on the SQL Server connection.
    /// 
    /// https://msdn.microsoft.com/en-us/library/ms190763.aspx
    /// </summary>
    [Flags]
    public enum SqlServerOptions
    {
        None = 0,
        DISABLE_DEF_CNST_CHK = 1, // Controls interim or deferred constraint checking.
        IMPLICIT_TRANSACTIONS = 2, // For dblib network library connections, controls whether a transaction is started implicitly when a statement is executed. The IMPLICIT_TRANSACTIONS setting has no effect on ODBC or OLEDB connections.
        CURSOR_CLOSE_ON_COMMIT = 4, // Controls behavior of cursors after a commit operation has been performed.
        ANSI_WARNINGS = 8, // Controls truncation and NULL in aggregate warnings.
        ANSI_PADDING = 16, // Controls padding of fixed-length variables.
        ANSI_NULLS = 32, // Controls NULL handling when using equality operators.
        ARITHABORT = 64, // Terminates a query when an overflow or divide-by-zero error occurs during query execution.
        ARITHIGNORE = 128, // Returns NULL when an overflow or divide-by-zero error occurs during a query.
        QUOTED_IDENTIFIER = 256, // Differentiates between single and double quotation marks when evaluating an expression.
        NOCOUNT = 512, // Turns off the message returned at the end of each statement that states how many rows were affected.
        ANSI_NULL_DFLT_ON = 1024, // Alters the session's behavior to use ANSI compatibility for nullability. New columns defined without explicit nullability are defined to allow nulls.
        ANSI_NULL_DFLT_OFF = 2048, // Alters the session's behavior not to use ANSI compatibility for nullability. New columns defined without explicit nullability do not allow nulls.
        CONCAT_NULL_YIELDS_NULL = 4096, // Returns NULL when concatenating a NULL value with a string.
        NUMERIC_ROUNDABORT = 8192, // Generates an error when a loss of precision occurs in an expression.
        XACT_ABORT = 16384 // Rolls back a transaction if a Transact-SQL statement raises a run-time error.
    }
}
