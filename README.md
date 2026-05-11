# FilterSortParsing

A .NET library for parsing and applying OData-like filter and sorting expressions to `IQueryable<T>` data sources.

## Features

- **Filter Support** - OData-like filter syntax with comparison operators, string functions, and logical operators
- **Sorting Support** - Multi-field sorting with ascending/descending order
- **Case Insensitive** - Keywords, operators, and property names
- **Nested Properties** - Dot notation navigation

## Installation

```xml
<PackageReference Include="Soukoku.FilterSortParsing" Version="*" />
```

## Quick Start

### Filtering
```csharp
using Soukoku.FilterSortParsing;

var people = dbContext.People.AsQueryable();

// Simple filter
var adults = people.ApplyFilter("Age ge 18");

// Complex filter
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
```

### Chaining
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
- `eq`, `ne` - Equal, not equal
- `gt`, `ge` - Greater than (or equal)
- `lt`, `le` - Less than (or equal)

### String Functions
- `contains`, `startswith`, `endswith`

### Date/Time Values

- Date/time properties accept either textual date values (quoted strings) or numeric Unix epoch seconds (unquoted integers).
  - Quoted/text values are parsed using DateTime.Parse / DateTimeOffset.Parse (for example: `Created eq '2023-01-02'`).
  - Unquoted integer values are interpreted as Unix epoch seconds (seconds since 1970-01-01T00:00:00Z) and converted to UTC DateTime/DateTimeOffset. Example: `Created eq 1672531200`.
  - The `in` operator also accepts epoch values: `Created in (1672531200, 1672704000)`.

- Important: this library currently supports epoch seconds only (not milliseconds). Use quoted ISO strings or epoch seconds to avoid ambiguity.


### Logical
- `and`, `or`, `not`
- `( )` - Grouping and precedence

## Sorting Syntax

- **Direction**: `asc`, `ascending`, `desc`, `descending`
- **Multiple fields**: Comma-separated
- **Default**: Ascending

## Documentation

- **[Filter Documentation](FILTER.md)** - Complete filter syntax and examples
- **[OrderBy Documentation](ORDERBY.md)** - Complete sorting syntax

## Compatibility

- **.NET 8+** - Full support
- **.NET Framework 4.6.2+** - Full support with System.Memory backport
