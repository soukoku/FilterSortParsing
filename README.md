# FilterSortParsing

A .NET library for parsing and applying OData-like filter and sorting expressions to `IQueryable<T>` data sources.

## Features

- **Filter Support** - OData-like filter syntax with comparison operators, string functions, and logical operators
- **Sorting Support** - Multi-field sorting with ascending/descending order
- **High Performance** - Span-based parsing with minimal memory allocations
- **Case Insensitive** - Keywords, operators, and property names
- **Nested Properties** - Full support for dot notation navigation

## Installation

```xml
<!-- Add to your .csproj file -->
<PackageReference Include="Soukoku.FilterSortParsing" Version="*" />
```

## Quick Start

### Filtering
```csharp
using Soukoku.FilterSortParsing;

var people = dbContext.People.AsQueryable();

// Simple filter
var adults = people.ApplyFilter("Age ge 18");

// Complex filter with multiple conditions
var result = people.ApplyFilter(
    "(Age ge 25 and Age le 65) and (Address.State eq 'CA' or Address.State eq 'NY')"
);

// String operations
var johns = people.ApplyFilter("FirstName startswith 'John'");
```

### Sorting
```csharp
// Single field
var sorted = people.ApplyOrderBy("LastName");

// Multiple fields with directions
var sorted = people.ApplyOrderBy("Age desc, LastName asc, FirstName asc");

// Nested properties
var sorted = people.ApplyOrderBy("Address.State, Address.City, LastName");
```

### Chaining Operations
```csharp
var result = people
    .ApplyFilter("Age ge 25 and IsActive eq true")
    .ApplyOrderBy("LastName, FirstName")
    .Skip(20)
    .Take(10)
    .ToList();
```

## Filter Operators

### Comparison
- `eq` - Equal
- `ne` - Not equal
- `gt`, `ge` - Greater than (or equal)
- `lt`, `le` - Less than (or equal)

### String Functions
- `contains` - Contains substring
- `startswith` - Starts with substring
- `endswith` - Ends with substring

### Logical
- `and` - Logical AND
- `or` - Logical OR
- `not` - Logical NOT

### Grouping
- `( )` - Parentheses for grouping and precedence control

## Sorting Syntax

- **Direction keywords**: `asc`, `ascending`, `desc`, `descending`
- **Multiple fields**: Comma-separated list
- **Default**: Ascending if no direction specified

## Examples

### Filter Examples
```csharp
// Equality
people.ApplyFilter("Age eq 30")

// Range
people.ApplyFilter("Age ge 18 and Age le 65")

// String matching
people.ApplyFilter("Email contains '@gmail.com'")

// Complex logic
people.ApplyFilter("(Age lt 30 or Age gt 60) and IsActive eq true")

// Negation
people.ApplyFilter("not (Status eq 'Inactive')")

// Nested properties
people.ApplyFilter("Address.City eq 'Seattle' and Address.State eq 'WA'")
```

### Sort Examples
```csharp
// Single field
people.ApplyOrderBy("CreatedDate desc")

// Multiple fields
people.ApplyOrderBy("Department asc, Salary desc, LastName asc")

// Nested properties
people.ApplyOrderBy("Company.Name, Department, LastName")
```

## Documentation

Comprehensive documentation is available:

- **[Filter Documentation](FILTER.md)** - Complete filter syntax, operators, and examples
- **[OrderBy Documentation](ORDERBY.md)** - Complete sorting syntax and usage

## Compatibility

- **.NET 8+** - Full support
- **.NET Framework 4.6.2+** - Full support with System.Memory backport
