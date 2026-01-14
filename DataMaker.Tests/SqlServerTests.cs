using System.Diagnostics;

namespace DataMaker.Tests;

public class Instrument
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Manufacturer { get; set; }
    public string ProductNumber { get; set; }
    public string OrderNumber { get; set; }
}

public class SetType
{
    public string Name { get; set; }
}

public class SqlServerTests
{
    private Generator GenerateInstrumentMaker()
    {
        var generator = new Generator();
        generator.AddProvider(new SqlServerDataProvider("Data Source=DESKTOP-PGERLRJ\\SQLEXPRESS;Database=SpmDataNetBigBoy;Integrated Security=True;TrustServerCertificate=True"));
        generator.AddDataMap<Instrument>("Instrument")
            .WithColumn(i => i.Name, "Name")
            .WithColumn(i => i.ProductNumber, "ProdNum1")
            .WithLookup(i => i.Manufacturer, "Mfg", "Mfg1Id", "Id", "Name");

        return generator;
    }

    private Generator GenerateUserMaker()
    {
        var generator = new Generator();
        generator.AddProvider(new SqlServerDataProvider("Server=.;Database=TestDb;Trusted_Connection=True;TrustServerCertificate=True"));
        generator.AddDataMap<User>("User")
            .WithColumn(u => u.FullName, "Name")
            .WithLookup(u => u.City, "Address", "AddressId", "Id", "City");

        return generator;
    }

    [Fact]
    public void SequentialInstrumentSelectionTest_ShouldPass()
    {
        var generator = GenerateInstrumentMaker();

        // generate 10 instruments from the start of the table
        var sequentialInstruments = generator.Generate<Instrument>(10, SelectionStrategy.Random).ToList();

        Debug.WriteLine($"| {"Name",-60} | {"Manufacturer",-20} | {"ProductNumber",-15} |");
        foreach (var instrument in sequentialInstruments)
        {
            Debug.WriteLine($"| {instrument.Name,-60} | {instrument.Manufacturer,-20} | {instrument.ProductNumber,-15} |");
            Assert.False(string.IsNullOrEmpty(instrument.Name));
            Assert.False(string.IsNullOrEmpty(instrument.Manufacturer));
            Assert.False(string.IsNullOrEmpty(instrument.ProductNumber));
        }
    }

    [Fact]
    public void SequentialUserSelectionTest_ShouldPass()
    {
        var generator = GenerateUserMaker();

        // generate 10 users from the start of the table
        var sequentialUsers = generator.Generate<User>(10).ToList();
    }

    [Fact]
    public void RandomInstrumentSelectionTest_ShouldPass()
    {
        var generator = new Generator();
        generator.AddProvider(new SqlServerDataProvider("Data Source=DESKTOP-PGERLRJ\\SQLEXPRESS;Database=SpmDataNetBigBoy;Integrated Security=True;TrustServerCertificate=True"));
        generator.AddDataMap<Instrument>("Instrument")
            .WithColumn(i => i.Name, "Name")
            .WithColumn(i => i.ProductNumber, "ProdNum1")
            .WithLookup(i => i.Manufacturer, "Mfg", "Mfg1Id", "Id", "Name");

        //  generate 10 random instruments
        var randomInstruments = generator.Generate<Instrument>(10, SelectionStrategy.Random).ToList();
    }

    [Fact]
    public void SequentialIdGenerationTest_ShouldPass()
    {
        var generator = new Generator();
        generator.AddProvider(new SqlServerDataProvider("Data Source=DESKTOP-PGERLRJ\\SQLEXPRESS;Database=SpmDataNetBigBoy;Integrated Security=True;TrustServerCertificate=True"));

        // Test WithSequence for simple sequential IDs
        generator.AddDataMap<Instrument>("Instrument")
            .WithSequence(i => i.Id)  // Generate IDs: 1, 2, 3...
            .WithColumn(i => i.Name, "Name")
            .WithColumn(i => i.ProductNumber, "ProdNum1");

        var instruments = generator.Generate<Instrument>(20).ToList();

        // Verify IDs are sequential starting from 1
        for (int i = 0; i < instruments.Count; i++)
        {
            Assert.Equal(i + 1, instruments[i].Id);
            Assert.False(string.IsNullOrEmpty(instruments[i].Name));
        }

        Debug.WriteLine("\nSequential ID Test Results:");
        Debug.WriteLine($"| {"ID",-5} | {"Name",-60} |");
        foreach (var instrument in instruments.Take(10))
        {
            Debug.WriteLine($"| {instrument.Id,-5} | {instrument.Name,-60} |");
        }
    }

    [Fact]
    public void CustomSequentialFormattingTest_ShouldPass()
    {
        var generator = new Generator();
        generator.AddProvider(new SqlServerDataProvider("Data Source=DESKTOP-PGERLRJ\\SQLEXPRESS;Database=SpmDataNetBigBoy;Integrated Security=True;TrustServerCertificate=True"));

        // Test WithTableMap with index for custom formatted values
        generator.AddDataMap<Instrument>("Instrument")
            .WithSequence(i => i.Id, startValue: 1000)  // Start at 1000
            .WithTableMap(i => i.OrderNumber, (row, provider, index) => $"ORD-{index + 1:D5}")  // Custom format
            .WithColumn(i => i.Name, "Name");

        var instruments = generator.Generate<Instrument>(10).ToList();

        // Verify custom formatting
        for (int i = 0; i < instruments.Count; i++)
        {
            Assert.Equal(1000 + i, instruments[i].Id);  // IDs: 1000, 1001, 1002...
            Assert.Equal($"ORD-{i + 1:D5}", instruments[i].OrderNumber);  // ORD-00001, ORD-00002...
            Assert.False(string.IsNullOrEmpty(instruments[i].Name));
        }

        Debug.WriteLine("\nCustom Formatting Test Results:");
        Debug.WriteLine($"| {"ID",-5} | {"OrderNumber",-12} | {"Name",-50} |");
        foreach (var instrument in instruments)
        {
            Debug.WriteLine($"| {instrument.Id,-5} | {instrument.OrderNumber,-12} | {instrument.Name,-50} |");
        }
    }
}