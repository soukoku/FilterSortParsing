namespace Soukoku.FilterSortParsing.Tests;

public enum Status
{
    Active,
    Inactive,
    Pending
}

public class Person
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
    public int ComparisonAge { get; set; }
    public Status Status { get; set; }
    public Address? Address { get; set; }
}

public class Address
{
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public int ZipCode { get; set; }
}
