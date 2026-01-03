using System.Linq;

namespace Soukoku.FilterSortParsing;

/// <summary>
/// Provides extension methods for applying OData-like filter and sorting operations to <see cref="IQueryable{T}"/> sources.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies an OData-like filter expression to the queryable source.
    /// </summary>
    /// <typeparam name="T">The type of elements in the queryable source.</typeparam>
    /// <param name="source">The queryable source to filter.</param>
    /// <param name="filter">
    /// The filter expression string in OData-like syntax. Supports comparison operators (eq, ne, gt, ge, lt, le),
    /// string functions (contains, startswith, endswith), logical operators (and, or, not), and grouping with parentheses.
    /// Property names and operators are case-insensitive.
    /// </param>
    /// <returns>A filtered <see cref="IQueryable{T}"/> based on the specified filter expression.</returns>
    /// <exception cref="System.ArgumentException">Thrown when an invalid property name is specified in the filter.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when the filter syntax is invalid or type conversion fails.</exception>
    /// <example>
    /// <code>
    /// // Simple equality
    /// var adults = people.ApplyFilter("Age ge 18");
    /// 
    /// // Complex filter with multiple conditions
    /// var result = people.ApplyFilter("(Age ge 25 and Age le 65) and Address.State eq 'CA'");
    /// 
    /// // String operations
    /// var johns = people.ApplyFilter("FirstName startswith 'John'");
    /// </code>
    /// </example>
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return source;
        }
        
        var filterExpression = FilterParser.Parse(filter);
        return FilterApplier.ApplyFilter(source, filterExpression);
    }

    /// <summary>
    /// Applies an OData-like sorting expression to the queryable source.
    /// </summary>
    /// <typeparam name="T">The type of elements in the queryable source.</typeparam>
    /// <param name="source">The queryable source to sort.</param>
    /// <param name="orderBy">
    /// The sorting expression string in OData-like syntax. Supports comma-separated fields with optional
    /// direction keywords (asc, ascending, desc, descending). Property names are case-insensitive.
    /// Default direction is ascending if not specified.
    /// </param>
    /// <returns>An ordered <see cref="IQueryable{T}"/> based on the specified sorting expression.</returns>
    /// <exception cref="System.ArgumentException">Thrown when an invalid property name or direction keyword is specified.</exception>
    /// <example>
    /// <code>
    /// // Single field ascending
    /// var sorted = people.ApplyOrderBy("LastName");
    /// 
    /// // Multiple fields with directions
    /// var sorted = people.ApplyOrderBy("Age desc, LastName asc, FirstName asc");
    /// 
    /// // Nested properties
    /// var sorted = people.ApplyOrderBy("Address.State, Address.City");
    /// </code>
    /// </example>
    public static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> source, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }
        
        var clauses = OrderByParser.Parse(orderBy!);
        return OrderByApplier.ApplyOrderBy(source, clauses);
    }
}
