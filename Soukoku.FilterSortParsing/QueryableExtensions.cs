using System.Linq;

namespace Soukoku.FilterSortParsing;

/// <summary>
/// Extension methods for applying OData-like filter and sorting operations to <see cref="IQueryable{T}"/>.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies an OData-like filter expression to the queryable source.
    /// </summary>
    /// <typeparam name="T">The type of elements in the queryable source.</typeparam>
    /// <param name="source">The queryable source to filter.</param>
    /// <param name="filter">
    /// Filter expression in OData-like syntax. Supports comparison operators (eq, ne, gt, ge, lt, le),
    /// string functions (contains, startswith, endswith), logical operators (and, or, not), and grouping.
    /// Case-insensitive.
    /// </param>
    /// <returns>A filtered <see cref="IQueryable{T}"/>.</returns>
    /// <exception cref="System.ArgumentException">Thrown when an invalid property name is specified.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when the filter syntax is invalid.</exception>
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
    /// Sorting expression in OData-like syntax. Supports comma-separated fields with optional
    /// direction keywords (asc, ascending, desc, descending). Default direction is ascending.
    /// Case-insensitive.
    /// </param>
    /// <returns>An ordered <see cref="IQueryable{T}"/>.</returns>
    /// <exception cref="System.ArgumentException">Thrown when an invalid property or direction is specified.</exception>
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
