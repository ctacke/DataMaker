# DataMaker 

<img src="./icon.png" alt="DataMaker Logo" width="100" />

A .NET library for quickly generating test data entities from existing data sources.

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

DataMaker simplifies test data generation by mapping your existing database tables (or other data sources) to strongly-typed C# entities. Instead of writing complex test data setup code, define your mappings once and generate as many test instances as you need.

### Key Features

- **Strongly-typed mappings** using lambda expressions
- **Foreign key lookups** - automatically resolve relationships between tables
- **Multiple selection strategies** - sequential or random data selection
- **Provider pattern** - easily extend to support different data sources (SQL Server, CSV, etc.)
- **Fluent API** - clean, readable configuration
- **Type-safe** - compile-time validation of your mappings

## Installation

### NuGet Package

```bash
dotnet add package DataMaker
```

Or via Package Manager Console:

```powershell
Install-Package DataMaker
```

### Build from Source

```bash
git clone https://github.com/yourusername/DataMaker.git
cd DataMaker
dotnet build
```

## Quick Start

### Basic Usage

```csharp
using DataMaker;

// 1. Create a Generator instance
var generator = new Generator();

// 2. Add a data provider (SQL Server, CSV, etc.)
generator.AddProvider(new SqlServerDataProvider("YourConnectionString"));

// 3. Configure mappings for your entity
generator.AddDataMap<User>("Users")  // Primary table name
    .WithColumn(u => u.Name, "Name")
    .WithColumn(u => u.Email, "Email");

// 4. Generate test data
var users = generator.Generate<User>(10);  // Generate 10 users

foreach (var user in users)
{
    Console.WriteLine($"{user.Name} - {user.Email}");
}
```

### Entity Definition

```csharp
public class User
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
}
```

## Core Concepts

### 1. Data Providers

Data providers implement `IDataProvider` and retrieve data from a source. DataMaker currently includes:

- **SqlServerDataProvider** - Retrieves data from SQL Server databases
- **SqliteDataProvider** - Retrieves data from SQLite databases

```csharp
// SQL Server
var sqlProvider = new SqlServerDataProvider(
    "Server=.;Database=TestDb;Integrated Security=True;"
);
generator.AddProvider(sqlProvider);

// SQLite - accepts file path or connection string
var sqliteProvider = new SqliteDataProvider("path/to/database.sqlite");
generator.AddProvider(sqliteProvider);
```

### 2. Primary Tables

Each entity maps to a primary table. When generating data, DataMaker selects rows from this table based on your chosen strategy.

```csharp
generator.AddDataMap<Product>("Products")  // "Products" is the primary table
```

### 3. Selection Strategies

Control how rows are selected from the primary table:

```csharp
// Sequential - rows 0, 1, 2, 3... (wraps around if count > rows)
var sequential = generator.Generate<User>(10, SelectionStrategy.Sequential);

// Random - randomly selected rows
var random = generator.Generate<User>(10, SelectionStrategy.Random);
```

## Mapping Methods

### WithColumn - Simple Column Mapping

Maps a property directly to a column in the primary table.

```csharp
generator.AddDataMap<User>("Users")
    .WithColumn(u => u.Name, "FullName")
    .WithColumn(u => u.Email, "EmailAddress");
```

### WithSequence - Sequential Value Generation

Generates sequential numeric values, perfect for IDs or counters.

```csharp
// Generate sequential IDs starting at 1: 1, 2, 3, 4...
generator.AddDataMap<User>("Users")
    .WithSequence(u => u.Id)
    .WithColumn(u => u.Name, "Name");

// Generate IDs starting at a specific value
generator.AddDataMap<Order>("Orders")
    .WithSequence(o => o.OrderId, startValue: 1000)  // 1000, 1001, 1002...
    .WithColumn(o => o.Description, "Description");
```

### WithLookup - Foreign Key Relationships

Automatically resolves foreign key relationships to related tables.

```csharp
generator.AddDataMap<User>("Users")
    .WithColumn(u => u.Name, "Name")
    .WithLookup(
        u => u.City,           // Target property
        "Addresses",           // Related table
        "AddressId",           // Foreign key in primary table (Users)
        "Id",                  // Primary key in related table (Addresses)
        "City"                 // Column to retrieve from related table
    );
```

**How it works:**
1. Selects a row from the `Users` table
2. Reads the `AddressId` value from that row
3. Finds the matching row in `Addresses` where `Id = AddressId`
4. Returns the `City` value from the matched address row

### WithTableMap - Custom Mappings

For complex scenarios, use custom mapping functions with access to the row and provider.

```csharp
generator.AddDataMap<User>("Users")
    .WithColumn(u => u.FirstName, "FirstName")
    .WithColumn(u => u.LastName, "LastName")
    .WithTableMap(
        u => u.FullName,
        (row, provider) => $"{row["FirstName"]} {row["LastName"]}"
    );
```

**With Index Access** - For custom sequential or computed values:

```csharp
generator.AddDataMap<Order>("Orders")
    .WithTableMap(o => o.OrderNumber, (row, provider, index) => $"ORD-{index + 1000:D5}")  // ORD-01000, ORD-01001...
    .WithTableMap(o => o.CustomId, (row, provider, index) => index + 1)  // Sequential: 1, 2, 3...
    .WithColumn(o => o.Description, "Description");
```

## Advanced Examples

### Sequential ID Generation

Generate entities with auto-incrementing IDs without pulling them from the database:

```csharp
public class Customer
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}

generator.AddDataMap<Customer>("Customers")
    .WithSequence(c => c.Id)  // Auto-generate: 1, 2, 3, 4...
    .WithColumn(c => c.Name, "Name")
    .WithColumn(c => c.Email, "Email");

var customers = generator.Generate<Customer>(100);
// Creates customers with IDs from 1 to 100
```

### Complex Entity with Multiple Lookups

```csharp
public class Order
{
    public string? OrderNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? ProductName { get; set; }
    public decimal? Price { get; set; }
}

generator.AddDataMap<Order>("Orders")
    .WithColumn(o => o.OrderNumber, "OrderNum")
    .WithLookup(o => o.CustomerName, "Customers", "CustomerId", "Id", "Name")
    .WithLookup(o => o.ProductName, "Products", "ProductId", "Id", "Name")
    .WithLookup(o => o.Price, "Products", "ProductId", "Id", "Price");
```

### Combining Multiple Tables

```csharp
generator.AddDataMap<Employee>("Employees")
    .WithColumn(e => e.EmployeeId, "Id")
    .WithColumn(e => e.FirstName, "FirstName")
    .WithColumn(e => e.LastName, "LastName")
    .WithLookup(e => e.DepartmentName, "Departments", "DeptId", "Id", "Name")
    .WithLookup(e => e.ManagerName, "Employees", "ManagerId", "Id", "FirstName")
    .WithLookup(e => e.City, "Addresses", "AddressId", "Id", "City")
    .WithLookup(e => e.State, "Addresses", "AddressId", "Id", "State");

var employees = generator.Generate<Employee>(50, SelectionStrategy.Random);
```

### Custom Data Transformation

```csharp
generator.AddDataMap<Product>("Products")
    .WithColumn(p => p.Name, "ProductName")
    .WithTableMap(
        p => p.DisplayName,
        (row, provider) =>
        {
            var name = row["ProductName"].ToString();
            var category = row["Category"].ToString();
            return $"{category} - {name}";
        }
    );
```

### Custom Sequential Formats

Generate formatted IDs, order numbers, or codes using the index:

```csharp
public class Invoice
{
    public int Id { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ReferenceCode { get; set; }
    public decimal? Amount { get; set; }
}

generator.AddDataMap<Invoice>("Invoices")
    .WithSequence(i => i.Id)  // Simple numeric: 1, 2, 3...
    .WithTableMap(
        i => i.InvoiceNumber,
        (row, provider, index) => $"INV-2024-{index + 1:D6}")  // INV-2024-000001, INV-2024-000002...
    .WithTableMap(
        i => i.ReferenceCode,
        (row, provider, index) =>
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            return $"{year}{month:D2}-{index + 1000}";  // 202401-1000, 202401-1001...
        })
    .WithColumn(i => i.Amount, "Amount");

var invoices = generator.Generate<Invoice>(50);
```

## API Reference

### Generator Class

| Method | Description |
|--------|-------------|
| `AddProvider(IDataProvider)` | Adds a data provider for retrieving data |
| `AddDataMap<T>(string tableName)` | Creates a mapping configuration for type T with specified primary table |
| `Generate<T>(int count, SelectionStrategy)` | Generates specified number of entities using the configured mappings |

### DataMap<T> Class

| Method | Description |
|--------|-------------|
| `WithColumn(property, columnName)` | Maps property to column in primary table |
| `WithSequence(property, startValue)` | Generates sequential numeric values (default starts at 1) |
| `WithLookup(property, table, fk, pk, column)` | Maps property using foreign key lookup |
| `WithTableMap(property, func)` | Maps property using custom function (with optional index parameter) |

### SelectionStrategy Enum

| Value | Description |
|-------|-------------|
| `Sequential` | Selects rows in order (0, 1, 2...), wraps around if needed |
| `Random` | Randomly selects rows from available data |

## Data Providers

### SQL Server Provider

```csharp
var provider = new SqlServerDataProvider(connectionString);
```

**Features:**
- Lazy loading and caching of table data
- SQL injection protection via table name validation
- Support for standard SQL Server table naming conventions

**Security:** Table names are validated using a whitelist pattern (alphanumeric, underscore, period, brackets only).

### Creating Custom Providers

Implement `IDataProvider` to support other data sources:

```csharp
public class CsvDataProvider : IDataProvider
{
    public IDataEntity this[string entityName]
    {
        get
        {
            // Load CSV file and return as IDataEntity
        }
    }
}
```

## Best Practices

1. **Reuse Generator instances** - Configure once, generate many times
2. **Use WithColumn for simple mappings** - More performant than custom functions
3. **Cache related table data** - Providers cache table data automatically
4. **Validate your mappings** - Run tests to ensure foreign keys resolve correctly
5. **Choose appropriate selection strategy** - Use Sequential for deterministic tests, Random for broader coverage

## Performance Considerations

- **Table caching**: Each table is loaded once and cached by the provider
- **Foreign key lookups**: Performed via LINQ queries on cached data
- **Large datasets**: Consider using views or filtered queries in your provider

## Troubleshooting

### "No data provider has been added"
Call `AddProvider()` before calling `Generate()`.

### "Primary table 'X' has no rows"
Ensure your database table contains data.

### Foreign key lookup returns null
- Verify the foreign key column name matches your database
- Ensure related table has matching records
- Check that key values match (types and values)

### Type conversion errors
DataMaker attempts automatic type conversion. For complex types, use `WithTableMap` with custom conversion logic.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Roadmap

- [ ] CSV data provider implementation
- [ ] JSON data provider
- [ ] Support for multiple data providers in single DataMaker instance
- [ ] Performance optimizations for large datasets
- [ ] Fluent assertion extensions for testing
- [ ] Table join support
- [ ] Data transformation pipelines

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or suggestions:
- Open an issue on [GitHub](https://github.com/yourusername/DataMaker/issues)
- Check existing issues for solutions
- Review the examples in the test project

## Acknowledgments

Built with ❤️ for the .NET testing community.
