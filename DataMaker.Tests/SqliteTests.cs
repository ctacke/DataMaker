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
            .WithPersonData(c => c.FirstName, (data) => data.FirstName)
            .WithColumn(c => c.LastName, "LastName")
            .WithColumn(c => c.Company, "Company")
            .WithColumn(c => c.Email, "Email")
            .WithColumn(c => c.Phone, "Phone")
            .WithAddressData(c => c.Address, (data) => $"{data.StreetAddress}, {data.City}, {data.State}");

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

        var customers = generator.Generate<Customer>(10, SelectionStrategy.Random).ToList();

        Assert.Equal(10, customers.Count);
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
    public void SqliteProvider_RandomSelection_WithNoRepeats()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithSequence(c => c.Id, startValue: 1000)
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName")
            .WithColumn(c => c.Company, "Company")
            .WithColumn(c => c.Email, "Email");

        var customers = generator.Generate<Customer>(5, SelectionStrategy.Random, allowRepeats: false).ToList();

        Assert.Equal(5, customers.Count);
        // Verify sequential IDs despite random data selection
        for (int i = 0; i < customers.Count; i++)
        {
            Assert.Equal(1000 + i, customers[i].Id);
        }

        // Verify that unique customers were selected
        var distinctCustomers = customers.DistinctBy(c => c.Email);
        Assert.Equal(5, distinctCustomers.Count());

        Debug.WriteLine("\nRandom Selection Test:");
        Debug.WriteLine($"| {"ID",-5} | {"FirstName",-15} | {"LastName",-15} |");
        foreach (var customer in customers.Take(5))
        {
            Debug.WriteLine($"| {customer.Id,-5} | {customer.FirstName,-15} | {customer.LastName,-15} |");
        }
    }

    [Fact]
    public void SqliteProvider_SequentialSelection_WithNoRepeats()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithSequence(c => c.Id, startValue: 1000)
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName")
            .WithColumn(c => c.Company, "Company")
            .WithColumn(c => c.Email, "Email");

        var customers = generator.Generate<Customer>(15, SelectionStrategy.Sequential, allowRepeats: false).ToList();

        // Even though 15 requested, there are only 13 Customers in the Sqlite database
        Assert.Equal(13, customers.Count);
        // Verify sequential IDs despite random data selection
        for (int i = 0; i < customers.Count; i++)
        {
            Assert.Equal(1000 + i, customers[i].Id);
        }

        // Verify that unique customers were selected
        var distinctCustomers = customers.DistinctBy(c => c.Email);
        Assert.Equal(13, distinctCustomers.Count());

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

    [Fact]
    public void WithColumnTransform_ShouldTransformValue()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Product>("Products")
            .WithColumn(p => p.Name)
            .WithColumnTransform(p => p.Description, "Name", (index, value) => $"Transformed: {value.ToString().Substring(0, 5)}");

        var products = generator.Generate<Product>(5).ToList();

        Assert.Equal(5, products.Count);
        Assert.All(products, p => Assert.StartsWith("Transformed:", p.Description));

        Debug.WriteLine("\nWithColumnTransform Test:");
        foreach (var product in products)
        {
            Debug.WriteLine($"Name: {product.Name}, Description: {product.Description}");
        }
    }

    [Fact]
    public void FirstAdd_WithSpecificItems()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer2>("Customers")
            .WithColumnTransform(c => c.Id, "Id", (index, value) =>
                value is byte[] bytes ? new Guid(bytes) : Guid.Empty)
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName");

        var g = Guid.Parse("23d8a8fc-645d-4f4e-945c-ad497154c0b3");

        // Use FirstAdd to ensure we add customers with exact ids
        var customers = generator
            .Generate<Customer2>(8, SelectionStrategy.Random, allowRepeats: false)
            .FirstAdd(c => c.Id == g)
            .ToList();

        Assert.Equal(8, customers.Count);
        Assert.Contains(customers, c => c.Id == g);

        Debug.WriteLine("\nFirstAdd Filter Test:");
        foreach (var customer in customers)
        {
            Debug.WriteLine($"ID: {customer.Id}, Name: {customer.FirstName} {customer.LastName}");
        }
    }

    [Fact]
    public void FirstAdd_ShouldFilterGeneratedItems()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithSequence(c => c.Id, startValue: 10)
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName");

        // Use FirstAdd to filter for customers with Id > 5
        var customers = generator
            .Generate<Customer>(5, SelectionStrategy.Sequential, allowRepeats: true)
            .FirstAdd(c => c.Id > 5)
            .ToList();

        Assert.Equal(5, customers.Count);
        Assert.All(customers, c => Assert.True(c.Id > 5));

        Debug.WriteLine("\nFirstAdd Filter Test:");
        foreach (var customer in customers)
        {
            Debug.WriteLine($"ID: {customer.Id}, Name: {customer.FirstName} {customer.LastName}");
        }
    }

    [Fact]
    public void FirstAdd_WithMultiplePredicates_ShouldApplyAll()
    {
        var generator = new Generator();
        generator.AddProvider(new SqliteDataProvider(DatabasePath));

        generator.AddDataMap<Customer>("Customers")
            .WithSequence(c => c.Id, startValue: 6)
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName");

        // Chain multiple FirstAdd predicates
        var customers = generator
            .Generate<Customer>(3, SelectionStrategy.Sequential, allowRepeats: true)
            .FirstAdd(c => c.Id > 2)
            .FirstAdd(c => c.Id < 10)
            .ToList();

        Assert.Equal(3, customers.Count);
        Assert.All(customers, c => Assert.True(c.Id > 2 && c.Id < 10));

        Debug.WriteLine("\nFirstAdd Multiple Predicates Test:");
        foreach (var customer in customers)
        {
            Debug.WriteLine($"ID: {customer.Id}, Name: {customer.FirstName} {customer.LastName}");
        }
    }

    [Fact]
    public void DeterministicIds_ShouldProduceSameSequenceWithSameSeed()
    {
        const int seed = 12345;

        // First generation
        var generator1 = new Generator();
        generator1.AddProvider(new SqliteDataProvider(DatabasePath));
        generator1.AddDataMap<CustomerWithDeterministicIds>("Customers")
            .WithDeterministicLong(c => c.Id, seed)
            .WithDeterministicGuid(c => c.ExternalId, seed)
            .WithDeterministicString(c => c.Code, seed, length: 8, prefix: "CUST-")
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName");

        var customers1 = generator1.Generate<CustomerWithDeterministicIds>(5).ToList();

        // Second generation with same seed - should produce identical IDs
        var generator2 = new Generator();
        generator2.AddProvider(new SqliteDataProvider(DatabasePath));
        generator2.AddDataMap<CustomerWithDeterministicIds>("Customers")
            .WithDeterministicLong(c => c.Id, seed)
            .WithDeterministicGuid(c => c.ExternalId, seed)
            .WithDeterministicString(c => c.Code, seed, length: 8, prefix: "CUST-")
            .WithColumn(c => c.FirstName, "FirstName")
            .WithColumn(c => c.LastName, "LastName");

        var customers2 = generator2.Generate<CustomerWithDeterministicIds>(5).ToList();

        // Verify same seed produces same IDs
        Assert.Equal(5, customers1.Count);
        Assert.Equal(5, customers2.Count);

        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(customers1[i].Id, customers2[i].Id);
            Assert.Equal(customers1[i].ExternalId, customers2[i].ExternalId);
            Assert.Equal(customers1[i].Code, customers2[i].Code);
        }

        // Verify IDs look valid
        Assert.All(customers1, c => Assert.True(c.Id > 0, "Long ID should be positive"));
        Assert.All(customers1, c => Assert.NotEqual(Guid.Empty, c.ExternalId));
        Assert.All(customers1, c => Assert.StartsWith("CUST-", c.Code));

        Debug.WriteLine("\nDeterministic IDs Test:");
        foreach (var customer in customers1)
        {
            Debug.WriteLine($"ID: {customer.Id}, ExternalId: {customer.ExternalId}, Code: {customer.Code}, Name: {customer.FirstName} {customer.LastName}");
        }
    }
}
