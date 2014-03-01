# Overview
SqlSiphon is an ADO.NET wrapper that simplifies the process of connecting to MS SQL Server, MySQL, PostgreSQL, and OleDB-compliant databases and executing stored procedures (for SQL Server and MySQL) or text queries on them. It's been running in one iteration or another in about 15 different production systems for the last 5 years.

The project's core ethos is pretty simple: keep everything to the most basic datatypes possible. Part of the motivation for the project is to abstract away the details of ADO.NET or any other particular data access API being used, to make for faster project development and easier serialization over Web services. Mapping method calls from C# to stored procedure calls in the database provides an extra layer of type safety as well, for protection against SQL Injection attacks. 

SqlSiphon is not an Object-Relational Mapping (ORM) system. It is a system for simplifying the strict use of relational SQL in Object-Oriented applications. The database dictates the structure of the class objects (which are not capable of creating relationship graphs) and method calls. This is in contrast to heavier ORMs such as Hibernate. Databases have a way of sticking around longer than programs, I thus believe the database should be the master.

## Database support
Currently, it has been tested and used regularly with:
* MS SQL Server:
  * 2005,
  * 2008,
  * 2012
* MySQL: 
  * 5.1,
  * 5.5
* MS Access 2010
* Some CSV and Excel documents (via the JET OleDB driver)
* PostgreSQL 9.2 (only basic functionality tested so far, but is probably good)
* Stored procedure synchronization system (SPs are largely application-specific, so they should live with the application in source control)
* Schema migration tool
* SQL Server table-value parameters
* PostgreSQL array parameters
* Bulk Insert

## Future Work
* Expand database support
    * SQL Server Compact Edition
    * Oracle
    * SQLite
    * SqlAnywhere
    * Generic ODBC drivers (code exists, but is untested)
* Automatic generation of Foreign Key relationships
* Postgres and MySQL schema management and migration
* Data migration tool
* Query structure analyzer
