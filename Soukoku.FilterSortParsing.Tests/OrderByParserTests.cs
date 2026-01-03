using System;
using Xunit;

namespace Soukoku.FilterSortParsing.Tests;

public class OrderByParserTests
{
    [Fact]
    public void Parse_NullOrEmpty_ReturnsEmptyList()
    {
        // Arrange & Act
        var resultNull = OrderByParser.Parse(null);
        var resultEmpty = OrderByParser.Parse("");
        var resultWhitespace = OrderByParser.Parse("   ");

        // Assert
        Assert.Empty(resultNull);
        Assert.Empty(resultEmpty);
        Assert.Empty(resultWhitespace);
    }

    [Fact]
    public void Parse_SingleFieldNoDirection_ReturnsAscending()
    {
        // Arrange & Act
        var result = OrderByParser.Parse("Name");

        // Assert
        Assert.Single(result);
        Assert.Equal("Name", result[0].PropertyName);
        Assert.False(result[0].IsDescending);
    }

    [Fact]
    public void Parse_SingleFieldAsc_ReturnsAscending()
    {
        // Arrange & Act
        var result = OrderByParser.Parse("Name asc");

        // Assert
        Assert.Single(result);
        Assert.Equal("Name", result[0].PropertyName);
        Assert.False(result[0].IsDescending);
    }

    [Fact]
    public void Parse_SingleFieldDesc_ReturnsDescending()
    {
        // Arrange & Act
        var result = OrderByParser.Parse("Name desc");

        // Assert
        Assert.Single(result);
        Assert.Equal("Name", result[0].PropertyName);
        Assert.True(result[0].IsDescending);
    }

    [Theory]
    [InlineData("Name ASC")]
    [InlineData("Name asc")]
    [InlineData("Name Asc")]
    [InlineData("Name aSc")]
    [InlineData("Name ascending")]
    [InlineData("Name ASCENDING")]
    [InlineData("Name Ascending")]
    public void Parse_AscendingKeywords_CaseInsensitive(string orderBy)
    {
        // Arrange & Act
        var result = OrderByParser.Parse(orderBy);

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsDescending);
    }

    [Theory]
    [InlineData("Name DESC")]
    [InlineData("Name desc")]
    [InlineData("Name Desc")]
    [InlineData("Name dEsC")]
    [InlineData("Name descending")]
    [InlineData("Name DESCENDING")]
    [InlineData("Name Descending")]
    public void Parse_DescendingKeywords_CaseInsensitive(string orderBy)
    {
        // Arrange & Act
        var result = OrderByParser.Parse(orderBy);

        // Assert
        Assert.Single(result);
        Assert.True(result[0].IsDescending);
    }

    [Fact]
    public void Parse_MultipleFields_ReturnsMultipleClauses()
    {
        // Arrange & Act
        var result = OrderByParser.Parse("LastName asc, FirstName desc, Age");

        // Assert
        Assert.Equal(3, result.Count);
        
        Assert.Equal("LastName", result[0].PropertyName);
        Assert.False(result[0].IsDescending);
        
        Assert.Equal("FirstName", result[1].PropertyName);
        Assert.True(result[1].IsDescending);
        
        Assert.Equal("Age", result[2].PropertyName);
        Assert.False(result[2].IsDescending);
    }

    [Fact]
    public void Parse_MultipleFieldsWithExtraSpaces_HandlesCorrectly()
    {
        // Arrange & Act
        var result = OrderByParser.Parse("  Name  asc  ,  Age  desc  ,  City  ");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Name", result[0].PropertyName);
        Assert.False(result[0].IsDescending);
        Assert.Equal("Age", result[1].PropertyName);
        Assert.True(result[1].IsDescending);
        Assert.Equal("City", result[2].PropertyName);
        Assert.False(result[2].IsDescending);
    }

    [Fact]
    public void Parse_NestedProperty_PreservesPropertyPath()
    {
        // Arrange & Act
        var result = OrderByParser.Parse("Address.City desc");

        // Assert
        Assert.Single(result);
        Assert.Equal("Address.City", result[0].PropertyName);
        Assert.True(result[0].IsDescending);
    }

    [Fact]
    public void Parse_InvalidDirection_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => OrderByParser.Parse("Name invalid"));
        Assert.Contains("Invalid order direction", exception.Message);
        Assert.Contains("invalid", exception.Message);
    }

    [Fact]
    public void Parse_EmptyClausesIgnored_WithCommas()
    {
        // Arrange & Act
        var result = OrderByParser.Parse("Name,,Age,,,");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Name", result[0].PropertyName);
        Assert.Equal("Age", result[1].PropertyName);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("NAME")]
    [InlineData("Name")]
    [InlineData("nAmE")]
    public void Parse_PropertyName_PreservesCase(string propertyName)
    {
        // Arrange & Act
        var result = OrderByParser.Parse(propertyName);

        // Assert
        Assert.Single(result);
        Assert.Equal(propertyName, result[0].PropertyName);
    }
}
