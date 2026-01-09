using System.Diagnostics;

namespace DataMaker.Tests;

public class SqliteTests
{
    private const string DatabasePath = "Inputs/Northwind.sqlite";

    [Fact]
    public void SqliteProvider_ShouldLoadCustomers()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithSequence(c => c.Id) // this just generates sequantial numeric IDs
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName")
            .WithColumn(c => c.Company, "Company")
            .WithColumn(c => c.Email, "Email")
            .WithColumn(c => c.Phone, "Phone")
            .WithColumn(c => c.Address, "Address");

        var customers = generator.Generate<Customer>(10).ToList();

        Assert.Equal(10, customers.Count);
        Assert.All(customers, c => Assert.False(string.IsNullOrEmpty(c.FirstName)));
        Assert.All(customers, c => Assert.False(string.IsNullOrEmpty(c.LastName)));

        Debug.WriteLine("\nSQLite Customer Test Results:");
        Debug.WriteLine($"| {"ID",-5} | {"FirstName",-15} | {"LastName",-15} | {"Company",-30} |");
        foreach (var customer in customers.Take(5))
        {
            Debug.WriteLine($"| {customer.Id,-5} | {customer.FirstName,-15} | {customer.LastName,-15} | {customer.Company,-30} |");
        }
    }

    [Fact]
    public void SqliteProvider_ShouldLoadProducts()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Product>("Products")
            .WithValue(p => p.Id, () => Guid.NewGuid()) // you can just set a value for the field
            .WithColumn(p => p.Name) // or pull from a column with the same name
            .WithColumn(p => p.Cost, "StandardCost") // or pull from a column with a different name
            .WithColumn(p => p.Price, "ListPrice");

        var products = generator.Generate<Product>(10).ToList();

        Assert.Equal(10, products.Count);
        Assert.All(products, p => Assert.False(string.IsNullOrEmpty(p.Name)));

        Debug.WriteLine("\nSQLite Product Test Results:");
        Debug.WriteLine($"| {"ID",-36} | {"ProductName",-30} | {"Cost",-10} | {"Price",-10} |");
        foreach (var product in products.Take(5))
        {
            Debug.WriteLine($"| {product.Id,-36} | {product.Name,-30} | {product.Cost,-10:F2} | {product.Price,-10:F2} |");
        }
    }

    [Fact]
    public void SqliteProvider_RandomSelection_ShouldWork()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithSequence(c => c.Id, startValue: 1000)
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName")
            .WithColumn(c => c.Company, "Company");

        var customers = generator.Generate<Customer>(15, SelectionStrategy.Random).ToList();

        Assert.Equal(15, customers.Count);
        // Verify sequential IDs despite random data selection
        for (int i = 0; i < customers.Count; i++)
        {
            Assert.Equal(1000 + i, customers[i].Id);
        }

        Debug.WriteLine("\nRandom Selection Test:");
        Debug.WriteLine($"| {"ID",-5} | {"FirstName",-15} | {"LastName",-15} |");
        foreach (var customer in customers.Take(5))
        {
            Debug.WriteLine($"| {customer.Id,-5} | {customer.FirstName,-15} | {customer.LastName,-15} |");
        }
    }

    [Fact]
    public void SqliteProvider_CustomFormatting_ShouldWork()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithSequence(c => c.Id)
            .WithTableMap(c => c.FirstName, (row, provider, index) => $"Customer-{index + 1:D3}")
            .WithColumn(c => c.Company, "Company")
            .WithTableMap(c => c.Email, (row, provider) => $"{row["FirstName"]}.{row["LastName"]}@example.com".ToLower());

        var customers = generator.Generate<Customer>(5).ToList();

        Assert.Equal(5, customers.Count);
        for (int i = 0; i < customers.Count; i++)
        {
            Assert.Equal($"Customer-{i + 1:D3}", customers[i].FirstName);
            Assert.Contains("@example.com", customers[i].Email);
        }

        Debug.WriteLine("\nCustom Formatting Test:");
        foreach (var customer in customers)
        {
            Debug.WriteLine($"ID: {customer.Id}, Name: {customer.FirstName}, Email: {customer.Email}");
        }
    }

    [Fact]
    public void SqliteProvider_FilePathConstructor_ShouldWork()
    {
        // Test that SqliteDataProvider accepts a file path without "Data Source=" prefix
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Product>("Products")
            .WithValue(p => p.Id, () => Guid.NewGuid())
            .WithColumn(p => p.Name, "Name");

        var products = generator.Generate<Product>(3).ToList();

        Assert.Equal(3, products.Count);
    }

    [Fact]
    public void SqliteProvider_ConnectionStringConstructor_ShouldWork()
    {
        // Test that SqliteDataProvider accepts a full connection string
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider($"Data Source={DatabasePath}"));

        generator.AddDataMap<Product>("Products")
            .WithValue(p => p.Id, () => Guid.NewGuid())
            .WithColumn(p => p.Name, "Name");

        var products = generator.Generate<Product>(3).ToList();

        Assert.Equal(3, products.Count);
    }

    [Fact]
    public void WithValue_IndexParameter_ShouldWork()
    {
        // Test WithValue with index parameter for generating sequential formatted values
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithValue(c => c.Id, index => index + 1)  // Sequential: 1, 2, 3...
            .WithValue(c => c.FirstName, index => $"Customer{index + 1:D3}")  // Customer001, Customer002...
            .WithValue(c => c.Email, index => $"user{index + 1}@test.com")  // user1@test.com, user2@test.com...
            .WithColumn(c => c.Company, "Company");

        var customers = generator.Generate<Customer>(5).ToList();

        Assert.Equal(5, customers.Count);

        // Verify index-based values
        for (int i = 0; i < customers.Count; i++)
        {
            Assert.Equal(i + 1, customers[i].Id);
            Assert.Equal($"Customer{i + 1:D3}", customers[i].FirstName);
            Assert.Equal($"user{i + 1}@test.com", customers[i].Email);
        }

        Debug.WriteLine("\nWithValue Index Parameter Test:");
        Debug.WriteLine($"| {"ID",-5} | {"FirstName",-15} | {"Email",-25} | {"Company",-30} |");
        foreach (var customer in customers)
        {
            Debug.WriteLine($"| {customer.Id,-5} | {customer.FirstName,-15} | {customer.Email,-25} | {customer.Company,-30} |");
        }
    }

    [Fact]
    public void WithColumn_NoColumnName_ShouldUsePropertyName()
    {
        // Test WithColumn without column name parameter - should use property name
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithSequence(c => c.Id)
            .WithColumn(c => c.FirstName)  // Should map to "FirstName" column
            .WithColumn(c => c.LastName)   // Should map to "LastName" column
            .WithColumn(c => c.Company);   // Should map to "Company" column

        var customers = generator.Generate<Customer>(5).ToList();

        Assert.Equal(5, customers.Count);
        Assert.All(customers, c => Assert.False(string.IsNullOrEmpty(c.FirstName)));
        Assert.All(customers, c => Assert.False(string.IsNullOrEmpty(c.LastName)));
        Assert.All(customers, c => Assert.False(string.IsNullOrEmpty(c.Company)));

        Debug.WriteLine("\nWithColumn Auto-Mapping Test:");
        Debug.WriteLine($"| {"ID",-5} | {"FirstName",-15} | {"LastName",-15} | {"Company",-30} |");
        foreach (var customer in customers)
        {
            Debug.WriteLine($"| {customer.Id,-5} | {customer.FirstName,-15} | {customer.LastName,-15} | {customer.Company,-30} |");
        }
    }
}
