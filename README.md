! Overview
SqlSiphon is an ADO.NET wrapper that simplifies the process of connecting to MS SQL Server, Oracle MySQL, and OLEDB-compliant databases and executing stored procedures (for SQL Server and MySQL) or text queries on them. It's been running in one iteration or another in about 10 different production systems for the last 5 years.

The project's core ethos is pretty simple: keep everything to the most basic datatypes possible. Part of the motivation for the project is to abstract away the details of ADO.NET or any other particular data access API being used, to make for faster project development and easier serialization over Web services. Mapping method calls from C# to stored procedure calls in the database provides an extra layer of type safety as well, for protection against SQL Injection attacks. 

As a very simple Object-Relational Mapping system, the database dictates the structure of the class objects and method calls. This is in contrast to heavier ORMs such as Hibernate. Databases have a way of sticking around longer than programs, so I felt it important that the DB be the master here.

! Database support
Currently, it has been tested and used regularly with MS SQL Server 2005, 2008, and 2012, MS Access 2010, and MySQL 5.1 and 5.5.

! Features
* *{"SqlSiphon.SqlClientDataAccessLayer"}*,
* *{"SqlSiphon.MySqlDataAccessLayer"}*,
* *{"SqlSiphon.OleDbDataAccessLayer"}*, these are abstract class from which one inherits to create a mostly automatic data access layer for the given database type (use SqlClientDataAccessLayer for SQL Server, MySqlDataAccessLayer for MySQL, and OleDbDataAccessLayer for Access. OleDbDataAccessLayer may work with other types of OLEDB-capable databases, I just haven't tested it yet). These are really just convenient, type-specific subclasses of the {"SqlSiphon.DataAcessLayer"} class, which is generic to any type of ADO.NET DbConnection. The class provides a number of protected methods that are intended to be called from your DAL class with methods that match the signature of store procedures in your database. Each method determines the name of the stored procedure to execute and the name of the parameters to pass to it based on the name and parameters of the method from which they are called.
* *{"void Execute(params object[] parameters)"}*: a method for executing a stored procedure without returning any results.
* *{"T Get<T>(params object[] parameters)"}*: executes a stored procedure and returns the first row from the result set. If T is a primitive type, the first parameter passed to the method is interpreted as the column name for the field to retrieve. If T is an "entity class" (a class with default constructor and publicly accessible properties), it maps the columns of the table to the fields in the class.
* *{"System.Collections.Generic.List<T> GetList<T>(params object[] parameters)"}*: executes a stored procedure and returns all rows from the result set as objects in a generic List. If T is a primitive type, the first parameter passed to the method is interpreted as the column name for the field to retrieve.  If T is an "entity class" (a class with default constructor and publicly accessible fields), it maps the columns of the table to the fields in the class.

! Future Work
* Expand database support
** SQL Server Compact Edition could be supported with little effort. 
** PostGresSQL
** Oracle