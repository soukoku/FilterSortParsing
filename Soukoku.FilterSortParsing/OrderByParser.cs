using System;
using System.Collections.Generic;
using System.Linq;

namespace Soukoku.FilterSortParsing;

/// <summary>
/// Parses OData-like orderBy expressions into structured <see cref="OrderByClause"/> objects.
/// Uses span-based parsing for optimal performance with minimal allocations.
/// </summary>
public class OrderByParser
{
    /// <summary>
    /// Parses an OData-like orderBy expression string into a list of <see cref="OrderByClause"/> objects.
    /// </summary>
    /// <param name="orderBy">
    /// The orderBy expression to parse. Supports comma-separated fields with optional direction keywords
    /// (asc, ascending, desc, descending). Returns an empty list if null or whitespace.
    /// </param>
    /// <returns>A list of <see cref="OrderByClause"/> objects representing the parsed sorting criteria.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid direction keyword is encountered.</exception>
    /// <example>
    /// <code>
    /// var clauses = OrderByParser.Parse("LastName asc, FirstName desc");
    /// // Returns: [{ PropertyName = "LastName", IsDescending = false }, { PropertyName = "FirstName", IsDescending = true }]
    /// </code>
    /// </example>
    public static List<OrderByClause> Parse(string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return new List<OrderByClause>();
        }

        var clauses = new List<OrderByClause>();
        ReadOnlySpan<char> span = orderBy.AsSpan();
        
        // Process each comma-separated clause
        while (!span.IsEmpty)
        {
            int commaIndex = span.IndexOf(',');
            ReadOnlySpan<char> clauseSpan;
            
            if (commaIndex >= 0)
            {
                clauseSpan = span.Slice(0, commaIndex);
                span = span.Slice(commaIndex + 1);
            }
            else
            {
                clauseSpan = span;
                span = ReadOnlySpan<char>.Empty;
            }
            
            clauseSpan = clauseSpan.Trim();
            
            if (clauseSpan.IsEmpty)
            {
                continue;
            }
            
            // Parse the property name and optional direction
            ParseClause(clauseSpan, clauses);
        }

        return clauses;
    }

    private static void ParseClause(ReadOnlySpan<char> clauseSpan, List<OrderByClause> clauses)
    {
        // Find first space to separate property name from direction
        int spaceIndex = clauseSpan.IndexOf(' ');
        
        ReadOnlySpan<char> propertySpan;
        ReadOnlySpan<char> directionSpan = ReadOnlySpan<char>.Empty;
        
        if (spaceIndex >= 0)
        {
            propertySpan = clauseSpan.Slice(0, spaceIndex).Trim();
            directionSpan = clauseSpan.Slice(spaceIndex + 1).Trim();
        }
        else
        {
            propertySpan = clauseSpan;
        }
        
        if (propertySpan.IsEmpty)
        {
            return;
        }
        
        // Allocate string only for the property name (needed for reflection later)
        string propertyName = propertySpan.ToString();
        bool isDescending = false;
        
        if (!directionSpan.IsEmpty)
        {
            // Check direction using span comparison (case-insensitive)
            if (MemoryExtensions.Equals(directionSpan, "desc".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                MemoryExtensions.Equals(directionSpan, "descending".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                isDescending = true;
            }
            else if (!MemoryExtensions.Equals(directionSpan, "asc".AsSpan(), StringComparison.OrdinalIgnoreCase) &&
                     !MemoryExtensions.Equals(directionSpan, "ascending".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                // Only allocate string for error message
                throw new ArgumentException($"Invalid order direction: {directionSpan.ToString()}. Expected 'asc' or 'desc'.");
            }
        }
        
        clauses.Add(new OrderByClause(propertyName, isDescending));
    }
}

/// <summary>
/// Represents a single sorting clause specifying a property name and sort direction.
/// </summary>
public class OrderByClause
{
    /// <summary>
    /// Gets the name of the property to sort by. Supports dot notation for nested properties (e.g., "Address.City").
    /// </summary>
    public string PropertyName { get; }
    
    /// <summary>
    /// Gets a value indicating whether the sort should be in descending order.
    /// Returns <c>false</c> for ascending order, <c>true</c> for descending order.
    /// </summary>
    public bool IsDescending { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderByClause"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property to sort by. Cannot be null.</param>
    /// <param name="isDescending">If set to <c>true</c>, sorts in descending order; otherwise, ascending.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyName"/> is null.</exception>
    public OrderByClause(string propertyName, bool isDescending)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        IsDescending = isDescending;
    }
}
