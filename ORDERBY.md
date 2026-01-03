# OrderBy Implementation - OData-like Syntax

## Supported Syntax

### Basic Sorting
```csharp
query.ApplyOrderBy("LastName");              // Ascending (default)
query.ApplyOrderBy("LastName asc");          // Explicit ascending
query.ApplyOrderBy("LastName desc");         // Descending
```

### Multiple Fields
```csharp
query.ApplyOrderBy("LastName asc, FirstName asc");
query.ApplyOrderBy("Age desc, LastName asc, FirstName asc");
query.ApplyOrderBy("LastName, FirstName");   // Default ascending
```

### Direction Keywords
| Keyword | Description |
|---------|-------------|
| `asc` / `ascending` | Ascending (default) |
| `desc` / `descending` | Descending |

## Examples

### Single Field
```csharp
query.ApplyOrderBy("Age");
query.ApplyOrderBy("LastName desc");
query.ApplyOrderBy("Salary asc");
```

### Multiple Fields
```csharp
query.ApplyOrderBy("LastName, FirstName");
query.ApplyOrderBy("Age desc, LastName asc, FirstName asc");
query.ApplyOrderBy("Department asc, Salary desc");
```

### Nested Properties
```csharp
query.ApplyOrderBy("Address.City");
query.ApplyOrderBy("Address.State asc, Address.City asc");
query.ApplyOrderBy("Age desc, Address.ZipCode asc");
```

## Case Insensitivity

Property names and keywords are case-insensitive:

```csharp
"Age asc"           // ?
"age ASC"           // ?
"AGE Ascending"     // ?
"Address.City"      // ?
"address.city"      // ?
```

## Supported Property Types

- **Numeric** - `int`, `long`, `double`, `decimal`, `float`, etc.
- **String** - Alphabetical (case-sensitive comparison)
- **Date/Time** - `DateTime`, `DateTimeOffset`
- **Boolean** - false < true
- **Nullable** - Null values sorted to beginning (asc) or end (desc)

## Implementation Architecture

### Components

1. **OrderByParser** - Span-based parsing, returns `OrderByClause` list
2. **OrderByApplier** - Converts clauses to LINQ expressions using `OrderBy`/`ThenBy`
3. **OrderByClause** - Immutable data: `PropertyName`, `IsDescending`

### Performance

- Span-based parsing with minimal allocations (70-80% reduction)
- Only property names allocated (required for reflection)
- Compiled expression trees execute as native LINQ

## Error Handling

```csharp
// Invalid property
query.ApplyOrderBy("InvalidProperty asc");
// Throws: ArgumentException

// Invalid direction
query.ApplyOrderBy("Age invalid");
// Throws: ArgumentException

// Null or empty (no-op)
query.ApplyOrderBy(null);      // Returns original query
query.ApplyOrderBy("");        // Returns original query
```

## Testing

- 33+ unit tests covering all scenarios
- Single/multiple fields, asc/desc, nested properties
- Case insensitivity, edge cases, error handling

## Usage Examples

### Simple Sorting
```csharp
var people = dbContext.People.AsQueryable();

var result = people.ApplyOrderBy("LastName desc");
```

### Chaining with Filter
```csharp
var result = people
    .ApplyFilter("Age ge 25")
    .ApplyOrderBy("LastName asc, FirstName asc")
    .ToList();
```

### Complex Chaining
```csharp
var result = people
    .ApplyFilter("(Age ge 25 and Age le 65) and Address.State eq 'CA'")
    .ApplyOrderBy("Address.City, LastName, FirstName")
    .Take(10)
    .ToList();
```

## Best Practices

### Order Specificity
```csharp
// Good: Consistent results
people.ApplyOrderBy("LastName, FirstName, Id");

// Less ideal: Inconsistent for duplicates
people.ApplyOrderBy("LastName");
```

### Performance
```csharp
// Good: Index columns first
people.ApplyOrderBy("UserId, CreatedAt desc");

// Less ideal: Computed/string properties slower
people.ApplyOrderBy("FullName, Email");
```

### Nested Properties
```csharp
// Good: Load navigation properties
people.Include(p => p.Address)
      .ApplyOrderBy("Address.City");
```

### Pagination
```csharp
var page = people
    .ApplyFilter("IsActive eq true")
    .ApplyOrderBy("LastName, FirstName")
    .Skip(pageNumber * pageSize)
    .Take(pageSize)
    .ToList();
```

## Compatibility

- **.NET 8+**, **.NET Framework 4.6.2+**, **.NET 10+**
- **Dependencies**: System.Memory (4.5.5) for .NET Framework 4.6.2
