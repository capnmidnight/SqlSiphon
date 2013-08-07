# Overview
SqlSiphon is an ADO.NET wrapper that simplifies the process of connecting to MS SQL Server, Oracle MySQL, and OLEDB-compliant databases and executing stored procedures (for SQL Server and MySQL) or text queries on them. It's been running in one iteration or another in about 10 different production systems for the last 5 years.

The project's core ethos is pretty simple: keep everything to the most basic datatypes possible. Part of the motivation for the project is to abstract away the details of ADO.NET or any other particular data access API being used, to make for faster project development and easier serialization over Web services. Mapping method calls from C# to stored procedure calls in the database provides an extra layer of type safety as well, for protection against SQL Injection attacks. 

As a very simple Object-Relational Mapping system, the database dictates the structure of the class objects and method calls. This is in contrast to heavier ORMs such as Hibernate. Databases have a way of sticking around longer than programs, so I felt it important that the DB be the master here.

## Database support
Currently, it has been tested and used regularly with:
* MS SQL Server:
    * 2005,
    * 2008,
    * 2012
* MS Access 2010
* Some CSV and Excel documents (via the JET OleDB driver)
* MySQL: 
  * 5.1,
  * 5.5

Additionally, it has provisional support (but does not yet have full production testing) for
* PostgreSQL 9.2

## Future Work
* Expand database support
    * SQL Server Compact Edition
    * Oracle
    * SQLite
    * SqlAnywhere
    * Generic ODBC drivers
