using System.Linq;
using Xunit;

namespace FilterSortParsing.Tests;

public class ApplyOrderByTests
{
    private IQueryable<Person> GetTestData()
    {
        return new[]
        {
            new Person 
            { 
                FirstName = "John", 
                LastName = "Doe", 
                Age = 30,
                Address = new Address { City = "New York", State = "NY", ZipCode = 10001 }
            },
            new Person 
            { 
                FirstName = "Jane", 
                LastName = "Smith", 
                Age = 25,
                Address = new Address { City = "Los Angeles", State = "CA", ZipCode = 90001 }
            },
            new Person 
            { 
                FirstName = "Bob", 
                LastName = "Johnson", 
                Age = 35,
                Address = new Address { City = "Chicago", State = "IL", ZipCode = 60601 }
            },
            new Person 
            { 
                FirstName = "Alice", 
                LastName = "Williams", 
                Age = 28,
                Address = new Address { City = "Houston", State = "TX", ZipCode = 77001 }
            },
            new Person 
            { 
                FirstName = "Charlie", 
                LastName = "Brown", 
                Age = 30,
                Address = new Address { City = "Phoenix", State = "AZ", ZipCode = 85001 }
            }
        }.AsQueryable();
    }

    [Fact]
    public void ApplyOrderBy_NullOrEmpty_ReturnsOriginal()
    {
        // Arrange
        var data = GetTestData();
        var originalList = data.ToList();

        // Act
        var resultNull = data.ApplyOrderBy(null).ToList();
        var resultEmpty = data.ApplyOrderBy("").ToList();
        var resultWhitespace = data.ApplyOrderBy("   ").ToList();

        // Assert
        Assert.Equal(originalList, resultNull);
        Assert.Equal(originalList, resultEmpty);
        Assert.Equal(originalList, resultWhitespace);
    }

    [Fact]
    public void ApplyOrderBy_SingleFieldAscending_OrdersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyOrderBy("Age").ToList();

        // Assert
        Assert.Equal(25, result[0].Age);
        Assert.Equal(28, result[1].Age);
        Assert.Equal(30, result[2].Age);
        Assert.Equal(30, result[3].Age);
        Assert.Equal(35, result[4].Age);
    }

    [Fact]
    public void ApplyOrderBy_SingleFieldDescending_OrdersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyOrderBy("Age desc").ToList();

        // Assert
        Assert.Equal(35, result[0].Age);
        Assert.Equal(30, result[1].Age);
        Assert.Equal(30, result[2].Age);
        Assert.Equal(28, result[3].Age);
        Assert.Equal(25, result[4].Age);
    }

    [Theory]
    [InlineData("age")]
    [InlineData("Age")]
    [InlineData("AGE")]
    [InlineData("aGe")]
    public void ApplyOrderBy_PropertyName_CaseInsensitive(string propertyName)
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyOrderBy(propertyName).ToList();

        // Assert - Should order by age regardless of case
        Assert.Equal(25, result[0].Age);
        Assert.Equal(28, result[1].Age);
        Assert.Equal(30, result[2].Age);
        Assert.Equal(30, result[3].Age);
        Assert.Equal(35, result[4].Age);
    }

    [Fact]
    public void ApplyOrderBy_MultipleFields_OrdersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act - Order by Age ascending, then FirstName descending
        var result = data.ApplyOrderBy("Age asc, FirstName desc").ToList();

        // Assert
        Assert.Equal("Jane", result[0].FirstName); // Age 25
        Assert.Equal("Alice", result[1].FirstName); // Age 28
        Assert.Equal("John", result[2].FirstName); // Age 30, J > C
        Assert.Equal("Charlie", result[3].FirstName); // Age 30
        Assert.Equal("Bob", result[4].FirstName); // Age 35
    }

    [Fact]
    public void ApplyOrderBy_NestedProperty_OrdersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyOrderBy("Address.City").ToList();

        // Assert
        Assert.Equal("Chicago", result[0].Address.City);
        Assert.Equal("Houston", result[1].Address.City);
        Assert.Equal("Los Angeles", result[2].Address.City);
        Assert.Equal("New York", result[3].Address.City);
        Assert.Equal("Phoenix", result[4].Address.City);
    }

    [Theory]
    [InlineData("address.city")]
    [InlineData("Address.City")]
    [InlineData("ADDRESS.CITY")]
    [InlineData("Address.city")]
    [InlineData("address.City")]
    public void ApplyOrderBy_NestedProperty_CaseInsensitive(string propertyPath)
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyOrderBy(propertyPath).ToList();

        // Assert - Should order by city regardless of case
        Assert.Equal("Chicago", result[0].Address.City);
        Assert.Equal("Houston", result[1].Address.City);
        Assert.Equal("Los Angeles", result[2].Address.City);
        Assert.Equal("New York", result[3].Address.City);
        Assert.Equal("Phoenix", result[4].Address.City);
    }

    [Fact]
    public void ApplyOrderBy_ComplexMultipleFields_OrdersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act - Order by Age desc, then Address.State asc, then FirstName asc
        var result = data.ApplyOrderBy("Age desc, Address.State asc, FirstName asc").ToList();

        // Assert
        Assert.Equal("Bob", result[0].FirstName); // Age 35
        Assert.Equal("Charlie", result[1].FirstName); // Age 30, AZ < NY
        Assert.Equal("John", result[2].FirstName); // Age 30, NY
        Assert.Equal("Alice", result[3].FirstName); // Age 28
        Assert.Equal("Jane", result[4].FirstName); // Age 25
    }

    [Fact]
    public void ApplyOrderBy_StringField_OrdersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyOrderBy("LastName").ToList();

        // Assert
        Assert.Equal("Brown", result[0].LastName);
        Assert.Equal("Doe", result[1].LastName);
        Assert.Equal("Johnson", result[2].LastName);
        Assert.Equal("Smith", result[3].LastName);
        Assert.Equal("Williams", result[4].LastName);
    }

    [Fact]
    public void ApplyOrderBy_InvalidProperty_ThrowsException()
    {
        // Arrange
        var data = GetTestData();

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => data.ApplyOrderBy("InvalidProperty").ToList());
    }

    [Fact]
    public void ApplyOrderBy_InvalidNestedProperty_ThrowsException()
    {
        // Arrange
        var data = GetTestData();

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => data.ApplyOrderBy("Address.InvalidProperty").ToList());
    }

    [Theory]
    [InlineData("FirstName ASC, LastName DESC")]
    [InlineData("firstname asc, lastname desc")]
    [InlineData("FIRSTNAME Ascending, LASTNAME Descending")]
    public void ApplyOrderBy_MixedCase_Everything_WorksCorrectly(string orderBy)
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyOrderBy(orderBy).ToList();

        // Assert - Should order correctly regardless of case
        Assert.Equal("Alice", result[0].FirstName);
        Assert.Equal("Williams", result[0].LastName);
        Assert.Equal("Bob", result[1].FirstName);
        Assert.Equal("Charlie", result[2].FirstName);
        Assert.Equal("Jane", result[3].FirstName);
        Assert.Equal("John", result[4].FirstName);
    }
}
