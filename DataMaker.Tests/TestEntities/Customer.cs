namespace DataMaker.Tests;

public class Customer
{
    public int Id { get; set; }
    public string? Address { get; set; }
    public string? Company { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class Customer2
{
    public Guid Id { get; set; }
    public string? Address { get; set; }
    public string? Company { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class CustomerWithDeterministicIds
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public string? Code { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
