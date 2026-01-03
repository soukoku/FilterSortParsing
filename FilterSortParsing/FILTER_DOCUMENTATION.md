# Filter Implementation - OData-like Syntax

## Overview
The `ApplyFilter` method implements OData-like filter syntax with full support for comparison operators, string functions, logical operators, and grouping with parentheses.

## Supported Operators

### Comparison Operators
| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equal | `Age eq 30` |
| `ne` | Not equal | `Age ne 30` |
| `gt` | Greater than | `Age gt 30` |
| `ge` | Greater than or equal | `Age ge 30` |
| `lt` | Less than | `Age lt 30` |
| `le` | Less than or equal | `Age le 30` |

### String Functions
| Function | Description | Example |
|----------|-------------|---------|
| `contains` | String contains substring | `Name contains 'john'` |
| `startswith` | String starts with substring | `Name startswith 'J'` |
| `endswith` | String ends with substring | `Name endswith 'son'` |

### Logical Operators
| Operator | Description | Example |
|----------|-------------|---------|
| `and` | Logical AND | `Age eq 30 and Name eq 'John'` |
| `or` | Logical OR | `Age eq 30 or Age eq 25` |
| `not` | Logical NOT | `not Age eq 30` |

## Features

### 1. Basic Comparisons
```csharp
// Numeric comparison
query.ApplyFilter("Age eq 30");

// String comparison with quotes
query.ApplyFilter("FirstName eq 'John'");

// Nested property
query.ApplyFilter("Address.City eq 'New York'");
```

### 2. String Functions
```csharp
// Check if name contains substring
query.ApplyFilter("FirstName contains 'oh'");

// Check if name starts with letter
query.ApplyFilter("FirstName startswith 'J'");

// Check if name ends with suffix
query.ApplyFilter("LastName endswith 'son'");
```

### 3. Logical Operators
```csharp
// AND condition
query.ApplyFilter("Age ge 30 and FirstName eq 'John'");

// OR condition
query.ApplyFilter("Age eq 25 or Age eq 35");

// NOT condition
query.ApplyFilter("not Age eq 30");
```

### 4. Grouping with Parentheses
```csharp
// Simple grouping
query.ApplyFilter("(Age eq 30)");

// Complex grouping with operator precedence
query.ApplyFilter("(Age lt 30 or Age gt 30) and FirstName startswith 'J'");

// Nested grouping
query.ApplyFilter("Age eq 30 or (Age gt 25 and FirstName contains 'li')");

// NOT with grouping
query.ApplyFilter("not (Age lt 28 or Age gt 32)");
```

### 5. Complex Expressions
```csharp
// Multiple conditions with mixed operators
query.ApplyFilter("(Age ge 28 and Age le 30) and (Address.City contains 'o' or Address.State eq 'NY')");

// Deep nesting
query.ApplyFilter("((Age eq 30 and Name eq 'John') or (Age eq 25 and Name eq 'Jane')) and Address.City contains 'New'");
```

## Case Insensitivity

All keywords are case-insensitive:

```csharp
// Property names
"Age eq 30"      // ?
"age eq 30"      // ?
"AGE eq 30"      // ?

// Operators
"Age EQ 30"      // ?
"Age Eq 30"      // ?

// Logical operators
"Age eq 30 AND Name eq 'John'"  // ?
"Age eq 30 and Name eq 'John'"  // ?
"Age eq 30 And Name eq 'John'"  // ?
```

## Supported Value Types

### Strings
- Must be enclosed in single (`'`) or double (`"`) quotes
- Supports spaces: `"New York"`
- Example: `FirstName eq 'John Doe'`

### Numbers
- Integers: `Age eq 30`
- Decimals: `Price gt 99.99`

### Booleans
- `true` / `false` (case-insensitive)
- Example: `IsActive eq true`

### Null
- Keyword: `null` (case-insensitive)
- Example: `MiddleName eq null`

## Nested Properties

Navigate object graphs using dot notation:

```csharp
// Single level
query.ApplyFilter("Address.City eq 'Chicago'");

// String functions on nested properties
query.ApplyFilter("Address.City contains 'York'");

// Comparison on nested properties
query.ApplyFilter("Address.ZipCode gt 50000");
```

## Operator Precedence

1. **Parentheses** `()` - Highest priority
2. **NOT** `not`
3. **AND** `and`
4. **OR** `or` - Lowest priority

Examples:
```csharp
// Without parentheses: (A or B) and C
"A or B and C"

// With parentheses: A or (B and C)
"A or (B and C)"
```

## Implementation Architecture

### Components

1. **FilterTokenizer** (ref struct)
   - Lexical analysis with `ReadOnlySpan<char>`
   - Minimizes allocations during tokenization
   - Recognizes operators, values, properties, parentheses

2. **FilterParser**
   - Recursive descent parser
   - Builds expression tree from tokens
   - Handles operator precedence and grouping

3. **FilterApplier**
   - Converts filter expression tree to LINQ expressions
   - Type-safe with automatic type conversion
   - Generates efficient IQueryable predicates

### Performance Characteristics

- **Tokenization**: Span-based, minimal allocations
- **Parsing**: Single-pass with recursive descent
- **Execution**: Compiled expression trees, executes as native LINQ

## Error Handling

### Invalid Property
```csharp
query.ApplyFilter("InvalidProperty eq 30");
// Throws: ArgumentException - Property 'InvalidProperty' not found
```

### Invalid Syntax
```csharp
query.ApplyFilter("Age 30");              // Missing operator
query.ApplyFilter("Age eq");              // Missing value
query.ApplyFilter("(Age eq 30");          // Missing closing parenthesis
// All throw: InvalidOperationException
```

### Type Conversion Errors
```csharp
query.ApplyFilter("Age eq 'not a number'");
// Throws: InvalidOperationException - Cannot convert to type
```

## Testing

### Test Coverage
- ? 40+ integration tests covering all operators
- ? Comparison operators (eq, ne, gt, ge, lt, le)
- ? String functions (contains, startswith, endswith)
- ? Logical operators (and, or, not)
- ? Parentheses and complex grouping
- ? Nested properties
- ? Case insensitivity
- ? Error handling
- ? Complex multi-condition scenarios

## Usage Examples

### Simple Filtering
```csharp
var people = dbContext.People.AsQueryable();

// Find people aged 30
var result = people.ApplyFilter("Age eq 30");

// Find people in New York
var result = people.ApplyFilter("Address.City eq 'New York'");
```

### Complex Filtering
```csharp
// Find people aged 28-30 in specific locations
var result = people.ApplyFilter(
    "(Age ge 28 and Age le 30) and (Address.City contains 'o' or Address.State eq 'NY')"
);

// Find people whose names start with J and are either young or in California
var result = people.ApplyFilter(
    "FirstName startswith 'J' and (Age lt 30 or Address.State eq 'CA')"
);
```

### Chaining with OrderBy
```csharp
var result = people
    .ApplyFilter("Age ge 25")
    .ApplyOrderBy("LastName asc, FirstName asc")
    .ToList();
```

## Compatibility

- **Target Frameworks**: .NET 8, .NET Framework 4.6.2
- **Dependencies**: 
  - System.Memory (4.5.5) for .NET Framework 4.6.2
  - System.Linq.Expressions
  - System.Linq.Queryable
