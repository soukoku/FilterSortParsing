# Filter Implementation - OData-like Syntax

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
| `contains` | Contains substring | `Name contains 'john'` |
| `startswith` | Starts with substring | `Name startswith 'J'` |
| `endswith` | Ends with substring | `Name endswith 'son'` |

### Logical Operators
| Operator | Description | Example |
|----------|-------------|---------|
| `and` | Logical AND | `Age eq 30 and Name eq 'John'` |
| `or` | Logical OR | `Age eq 30 or Age eq 25` |
| `not` | Logical NOT | `not Age eq 30` |

## Examples

### Basic Comparisons
```csharp
query.ApplyFilter("Age eq 30");
query.ApplyFilter("FirstName eq 'John'");
query.ApplyFilter("Address.City eq 'New York'");
```

### String Functions
```csharp
query.ApplyFilter("FirstName contains 'oh'");
query.ApplyFilter("FirstName startswith 'J'");
query.ApplyFilter("LastName endswith 'son'");
```

### Logical Operators
```csharp
query.ApplyFilter("Age ge 30 and FirstName eq 'John'");
query.ApplyFilter("Age eq 25 or Age eq 35");
query.ApplyFilter("not Age eq 30");
```

### Grouping with Parentheses
```csharp
query.ApplyFilter("(Age lt 30 or Age gt 30) and FirstName startswith 'J'");
query.ApplyFilter("Age eq 30 or (Age gt 25 and FirstName contains 'li')");
query.ApplyFilter("not (Age lt 28 or Age gt 32)");
```

### Complex Expressions
```csharp
query.ApplyFilter("(Age ge 28 and Age le 30) and (Address.City contains 'o' or Address.State eq 'NY')");
query.ApplyFilter("((Age eq 30 and Name eq 'John') or (Age eq 25 and Name eq 'Jane')) and Address.City contains 'New'");
```

## Case Insensitivity

Property names, operators, and logical keywords are case-insensitive:

```csharp
"Age eq 30"      // ?
"age EQ 30"      // ?
"AGE Eq 30"      // ?
"Age eq 30 AND Name eq 'John'"  // ?
```

## Supported Value Types

- **Strings** - Enclosed in single (`'`) or double (`"`) quotes: `FirstName eq 'John Doe'`
- **Numbers** - Integers or decimals: `Age eq 30`, `Price gt 99.99`
- **Booleans** - `true` / `false` (case-insensitive): `IsActive eq true`
- **Null** - `null` (case-insensitive): `MiddleName eq null`

## Nested Properties

Navigate object graphs using dot notation:

```csharp
query.ApplyFilter("Address.City eq 'Chicago'");
query.ApplyFilter("Address.City contains 'York'");
```

## Operator Precedence

1. Parentheses `()` - Highest
2. NOT `not`
3. AND `and`
4. OR `or` - Lowest

## Implementation Architecture

### Components

1. **FilterTokenizer** - Span-based lexical analysis with minimal allocations
2. **FilterParser** - Recursive descent parser with operator precedence
3. **FilterApplier** - Converts expression tree to type-safe LINQ expressions

### Performance

- Span-based tokenization with minimal allocations
- Single-pass parsing with recursive descent
- Compiled expression trees execute as native LINQ

## Error Handling

```csharp
// Invalid property
query.ApplyFilter("InvalidProperty eq 30");
// Throws: ArgumentException

// Invalid syntax
query.ApplyFilter("Age 30");              // Missing operator
query.ApplyFilter("(Age eq 30");          // Missing closing parenthesis
// Throws: InvalidOperationException

// Type conversion errors
query.ApplyFilter("Age eq 'not a number'");
// Throws: InvalidOperationException
```

## Testing

- 40+ integration tests covering all operators
- Comparison operators, string functions, logical operators
- Parentheses, nested properties, case insensitivity
- Error handling and complex scenarios

## Compatibility

- **.NET 8+**, **.NET Framework 4.6.2+**
- **Dependencies**: System.Memory (4.5.5) for .NET Framework 4.6.2
