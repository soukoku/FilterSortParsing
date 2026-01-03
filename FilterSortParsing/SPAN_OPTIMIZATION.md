# OrderByParser Span Optimization

## Overview
Optimized `OrderByParser` to use `ReadOnlySpan<char>` to minimize string allocations during parsing.

## Key Changes

### Before (Original Implementation)
```csharp
var parts = orderBy.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
foreach (var part in parts)
{
    var trimmedPart = part.Trim();
    var tokens = trimmedPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    var propertyName = tokens[0];
    var direction = tokens[1].ToLowerInvariant();
    // ... comparison logic
}
```

**Allocations per parse call:**
- String array for comma splits
- String array for space splits (per clause)
- Trimmed string copies (per clause)
- Lowercased direction string (per clause)

### After (Span-based Implementation)
```csharp
ReadOnlySpan<char> span = orderBy.AsSpan();
while (!span.IsEmpty)
{
    // Manual comma parsing with Slice
    ReadOnlySpan<char> clauseSpan = ...;
    clauseSpan = clauseSpan.Trim(); // No allocation
    
    // Manual space parsing with Slice
    ReadOnlySpan<char> propertySpan = ...;
    ReadOnlySpan<char> directionSpan = ...;
    
    // Case-insensitive comparison without allocations
    MemoryExtensions.Equals(directionSpan, "desc".AsSpan(), StringComparison.OrdinalIgnoreCase);
}
```

**Allocations per parse call:**
- Only ONE string allocation per clause (for the property name, required for reflection)
- Optional string allocation for error messages only

## Performance Benefits

### Allocation Reduction
- **No array allocations** from `Split()` operations
- **No intermediate string copies** from `Trim()` operations  
- **No lowercase conversions** from `ToLowerInvariant()`
- Property name string allocated only once per clause (unavoidable - needed for reflection)

### Example Savings
For input: `"Name asc, Age desc, Address.City"`

**Before:**
- 1 string array (3 elements) for comma split
- 3 string arrays (2-3 elements each) for space splits
- 3 trimmed strings
- 2 lowercased direction strings
- **Total: ~10-15 allocations**

**After:**
- 3 property name strings (required for reflection)
- **Total: 3 allocations**

### Memory Efficiency
- **70-80% reduction** in allocations for typical orderBy strings
- Better cache locality with span operations
- Reduced GC pressure

## Compatibility

### .NET 8
- Native `ReadOnlySpan<char>` support
- No additional dependencies

### .NET Framework 4.6.2
- Uses `System.Memory` NuGet package (v4.5.5)
- Provides backport of Span<T> and MemoryExtensions
- Same performance characteristics

## Testing
All 33 existing unit tests pass without modification:
- ? 19 OrderByParser tests
- ? 14 ApplyOrderBy integration tests
- ? Case insensitivity maintained
- ? Error handling preserved
- ? All edge cases covered

## Implementation Details

### Manual Parsing
- Uses `IndexOf()` to find delimiters (comma, space)
- Uses `Slice()` to extract subspans without allocation
- Uses `Trim()` on spans (no allocation)

### Case-Insensitive Comparison
- Uses `MemoryExtensions.Equals()` with `StringComparison.OrdinalIgnoreCase`
- Works on both .NET 8 and .NET Framework 4.6.2
- No string allocation for comparison

### String Allocation Strategy
- Property names must be strings (required by `Expression.Property()` in reflection)
- Direction comparison stays as spans
- Error messages allocate strings only when throwing exceptions
