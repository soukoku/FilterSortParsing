using System.Linq;
using Xunit;

namespace Soukoku.FilterSortParsing.Tests;

public class ApplyFilterTests
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
    public void ApplyFilter_NullOrEmpty_ReturnsOriginal()
    {
        // Arrange
        var data = GetTestData();
        var originalCount = data.Count();

        // Act
        var resultNull = data.ApplyFilter(null).Count();
        var resultEmpty = data.ApplyFilter("").Count();
        var resultWhitespace = data.ApplyFilter("   ").Count();

        // Assert
        Assert.Equal(originalCount, resultNull);
        Assert.Equal(originalCount, resultEmpty);
        Assert.Equal(originalCount, resultWhitespace);
    }

    #region Equality Operators

    [Fact]
    public void ApplyFilter_Eq_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Age eq 30").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(30, p.Age));
    }

    [Fact]
    public void ApplyFilter_Ne_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Age ne 30").ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.NotEqual(30, p.Age));
    }

    [Fact]
    public void ApplyFilter_StringEq_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("FirstName eq 'John'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
    }

    #endregion

    #region Comparison Operators

    [Fact]
    public void ApplyFilter_Gt_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Age gt 30").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(35, result[0].Age);
    }

    [Fact]
    public void ApplyFilter_Ge_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Age ge 30").ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.True(p.Age >= 30));
    }

    [Fact]
    public void ApplyFilter_Lt_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Age lt 30").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.True(p.Age < 30));
    }

    [Fact]
    public void ApplyFilter_Le_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Age le 30").ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.All(result, p => Assert.True(p.Age <= 30));
    }

    #endregion

    #region String Functions

    [Fact]
    public void ApplyFilter_Contains_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("FirstName contains 'oh'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
    }

    [Fact]
    public void ApplyFilter_StartsWith_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("FirstName startswith 'J'").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.FirstName == "John");
        Assert.Contains(result, p => p.FirstName == "Jane");
    }

    [Fact]
    public void ApplyFilter_EndsWith_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("LastName endswith 'son'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Johnson", result[0].LastName);
    }

    #endregion

    #region Logical Operators

    [Fact]
    public void ApplyFilter_And_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Age ge 30 and FirstName eq 'John'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
        Assert.Equal(30, result[0].Age);
    }

    [Fact]
    public void ApplyFilter_Or_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Age eq 25 or Age eq 35").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Age == 25);
        Assert.Contains(result, p => p.Age == 35);
    }

    [Fact]
    public void ApplyFilter_Not_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("not Age eq 30").ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.NotEqual(30, p.Age));
    }

    #endregion

    #region Grouping with Parentheses

    [Fact]
    public void ApplyFilter_Parentheses_SimpleGrouping()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("(Age eq 25)").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(25, result[0].Age);
    }

    [Fact]
    public void ApplyFilter_Parentheses_ComplexGrouping()
    {
        // Arrange
        var data = GetTestData();

        // Act - (Age < 30 OR Age > 30) AND FirstName starts with J
        var result = data.ApplyFilter("(Age lt 30 or Age gt 30) and FirstName startswith 'J'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Jane", result[0].FirstName);
        Assert.Equal(25, result[0].Age);
    }

    [Fact]
    public void ApplyFilter_Parentheses_NestedGrouping()
    {
        // Arrange
        var data = GetTestData();

        // Act - Age = 30 OR (Age > 25 AND FirstName contains 'li')
        var result = data.ApplyFilter("Age eq 30 or (Age gt 25 and FirstName contains 'li')").ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.FirstName == "John");
        Assert.Contains(result, p => p.FirstName == "Charlie");
        Assert.Contains(result, p => p.FirstName == "Alice");
    }

    [Fact]
    public void ApplyFilter_Parentheses_WithNot()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("not (Age lt 28 or Age gt 32)").ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.True(p.Age >= 28 && p.Age <= 32));
    }

    #endregion

    #region Nested Properties

    [Fact]
    public void ApplyFilter_NestedProperty_Eq()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Address.City eq 'Chicago'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Chicago", result[0].Address.City);
    }

    [Fact]
    public void ApplyFilter_NestedProperty_Contains()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Address.City contains 'York'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("New York", result[0].Address.City);
    }

    [Fact]
    public void ApplyFilter_NestedProperty_Comparison()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Address.ZipCode gt 50000").ToList();

        // Assert
        Assert.Equal(4, result.Count); // Los Angeles, Chicago, Houston, Phoenix
        Assert.All(result, p => Assert.True(p.Address.ZipCode > 50000));
    }

    #endregion

    #region Case Insensitivity

    [Theory]
    [InlineData("AGE eq 30")]
    [InlineData("age eq 30")]
    [InlineData("Age eq 30")]
    [InlineData("aGe eq 30")]
    public void ApplyFilter_PropertyName_CaseInsensitive(string filter)
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter(filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(30, p.Age));
    }

    [Theory]
    [InlineData("Age EQ 30")]
    [InlineData("Age Eq 30")]
    [InlineData("Age eQ 30")]
    public void ApplyFilter_Operator_CaseInsensitive(string filter)
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter(filter).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Theory]
    [InlineData("Age eq 30 AND FirstName eq 'John'")]
    [InlineData("Age eq 30 and FirstName eq 'John'")]
    [InlineData("Age eq 30 And FirstName eq 'John'")]
    public void ApplyFilter_LogicalOperator_CaseInsensitive(string filter)
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter(filter).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void ApplyFilter_ComplexExpression_MultipleConditions()
    {
        // Arrange
        var data = GetTestData();

        // Act - (Age >= 28 AND Age <= 30) AND (City contains 'o' OR State = 'NY')
        var result = data.ApplyFilter(
            "(Age ge 28 and Age le 30) and (Address.City contains 'o' or Address.State eq 'NY')"
        ).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.FirstName == "John");    // NY, age 30
        Assert.Contains(result, p => p.FirstName == "Alice");   // Houston has 'o', age 28
        Assert.Contains(result, p => p.FirstName == "Charlie"); // Phoenix has 'o', age 30
    }

    [Fact]
    public void ApplyFilter_QuotedStrings_WithSpaces()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("Address.City eq 'New York'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("New York", result[0].Address.City);
    }

    #endregion

    #region Error Handling

    [Fact]
    public void ApplyFilter_InvalidProperty_ThrowsException()
    {
        // Arrange
        var data = GetTestData();

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => data.ApplyFilter("InvalidProperty eq 30").ToList());
    }

    [Fact]
    public void ApplyFilter_InvalidNestedProperty_ThrowsException()
    {
        // Arrange
        var data = GetTestData();

        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => data.ApplyFilter("Address.InvalidProperty eq 30").ToList());
    }

    #endregion

    #region OData Function Call Syntax

    [Fact]
    public void ApplyFilter_FunctionSyntax_Contains_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("contains(FirstName, 'oh')").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_StartsWith_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("startswith(FirstName, 'J')").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.FirstName == "John");
        Assert.Contains(result, p => p.FirstName == "Jane");
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_EndsWith_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("endswith(LastName, 'son')").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Johnson", result[0].LastName);
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_NestedProperty_FiltersCorrectly()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("contains(Address.City, 'York')").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("New York", result[0].Address.City);
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_WithLogicalOperators()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("startswith(FirstName, 'J') and Age gt 25").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
        Assert.Equal(30, result[0].Age);
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_CaseInsensitive()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var resultLower = data.ApplyFilter("contains(FirstName, 'oh')").ToList();
        var resultUpper = data.ApplyFilter("CONTAINS(FirstName, 'oh')").ToList();
        var resultMixed = data.ApplyFilter("Contains(FirstName, 'oh')").ToList();

        // Assert
        Assert.Single(resultLower);
        Assert.Single(resultUpper);
        Assert.Single(resultMixed);
    }

    [Fact]
    public void ApplyFilter_MixedSyntax_InfixAndFunction()
    {
        // Arrange
        var data = GetTestData();

        // Act - Using both infix (startswith) and function syntax (contains)
        var result = data.ApplyFilter("FirstName startswith 'J' and contains(LastName, 'oe')").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].FirstName);
        Assert.Equal("Doe", result[0].LastName);
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_WithNot_Contains()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("not contains(FirstName, 'oh')").ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.All(result, p => Assert.NotEqual("John", p.FirstName));
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_WithNot_StartsWith()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("not startswith(FirstName, 'J')").ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, p => p.FirstName == "John");
        Assert.DoesNotContain(result, p => p.FirstName == "Jane");
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_WithNot_EndsWith()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("not endswith(LastName, 'son')").ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, p => p.LastName == "Johnson");
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_NotWithParentheses()
    {
        // Arrange
        var data = GetTestData();

        // Act - Find people whose FirstName does NOT contain 'oh' (which only John has)
        var result = data.ApplyFilter("not contains(FirstName, 'oh')").ToList();

        // Assert
        // John - contains 'oh' -> excluded
        // Jane - does NOT contain 'oh' -> included
        // Bob - does NOT contain 'oh' -> included
        // Alice - does NOT contain 'oh' -> included
        // Charlie - does NOT contain 'oh' -> included
        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, p => p.FirstName == "John");
        Assert.Contains(result, p => p.FirstName == "Jane");
        Assert.Contains(result, p => p.FirstName == "Bob");
        Assert.Contains(result, p => p.FirstName == "Alice");
        Assert.Contains(result, p => p.FirstName == "Charlie");
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_NotWithAnd()
    {
        // Arrange
        var data = GetTestData();

        // Act
        var result = data.ApplyFilter("not contains(FirstName, 'oh') and Age gt 25").ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.FirstName == "Bob");
        Assert.Contains(result, p => p.FirstName == "Alice");
        Assert.Contains(result, p => p.FirstName == "Charlie");
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_ComplexNotExpression()
    {
        // Arrange
        var data = GetTestData();

        // Act - Not (startswith 'J' and contains 'oh')
        var result = data.ApplyFilter("not (startswith(FirstName, 'J') and contains(FirstName, 'oh'))").ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, p => p.FirstName == "John"); // Only John matches both conditions
    }

    [Fact]
    public void ApplyFilter_FunctionSyntax_MixedNotWithInfixAndFunction()
    {
        // Arrange
        var data = GetTestData();

        // Act - Not using function syntax combined with infix operator
        var result = data.ApplyFilter("not contains(FirstName, 'oh') and LastName startswith 'B'").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Charlie", result[0].FirstName);
        Assert.Equal("Brown", result[0].LastName);
    }

    #endregion
}
