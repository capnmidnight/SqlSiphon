# Overview
SqlSiphon is an ADO.NET wrapper that simplifies the process of connecting to MS SQL Server, MySQL, PostgreSQL, and OleDB-compliant databases and executing stored procedures (for SQL Server and MySQL) or text queries on them. It's been running in one iteration or another in about 20 different production systems for the last 7 years.

The project's core ethos is pretty simple: keep everything to the most basic datatypes possible. SqlSiphon is not an Object-Relational Mapping (ORM) system. It is a system for simplifying the strict use of relational SQL in applications and assist in the management of database change.

## Features
* Succinct execution of arbitrary SQL through ADO.NET without disposable object leaks.
* Maintains naming and parameter typing between stored procedures in the database and application level calls of SPs.
* Maps query results to class objects for meaningful consumption.
* Synchronizes stored procedures between environments.
* Keeps query code managed with application code, in source control
* Assists in schema migration
* Bulk insert with:
  * SQL Server table-value parameters
  * PostgreSQL array parameters
  * 
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
