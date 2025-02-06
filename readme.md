# SqlWriter
A .NET library that uses fluent methods to build a strongly typed, parameterized TSQL CRUD statement from POCO a class that represents the database table.  The library only requires minor configuration by the user to label the associated table name with .  Produces parameters that work with ADO database and Dapper.

## Framework
- net9.0

## Dependencies
- Microsoft.Data.SqlClient

## Installation
```sh
$ dotnet add package TSqlWriter
```

## Initial setup
Use the `TableName` attribute to identify the table name of the associated POCO class.  The *ColumnSqlType* attribute can be added to properties to identify the associated *System.Data.SqlDbType* when building the parameters.
```cs
    [TableName("dbo.Properties")]
    public class Property
    {
        public int ID { get; set; }
        [ColumnSqlType(SqlDbType.VarChar)]
        public string Address { get; set; }
        [ColumnSqlType(SqlDbType.Date)]
        public DateTime? Date { get; set; }
        [ColumnSqlType(SqlDbType.SmallInt)]
        public int? Status { get; set; }
    }
```

## Basic example
To create a statement, use the Builder's helper methods to apply and build the desired query syntax.  Once the statement is complete, call the Builder's `CompiledSql()` method to get the compiled string result, and associated parameters.
```cs
using SqlWriter;
// Build basic Select statement using a single table.
IQuery<Property> statement = SqlWriters.Query<Property>()
    .Select(a => new { a.ID })
    .Where(a => a.ID == 2);

// Use the GetSqlStatement method to compile the SQL statement.
string result = statement.GetSqlStatement();
```
SQL result:
```sql
SELECT a.ID FROM dbo.Properties AS a WHERE a.ID = @pw0;
```