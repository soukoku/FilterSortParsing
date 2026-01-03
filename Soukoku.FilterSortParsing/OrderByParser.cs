using System;
using System.Collections.Generic;
using System.Linq;

namespace Soukoku.FilterSortParsing;

public class OrderByParser
{
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

public class OrderByClause
{
    public string PropertyName { get; }
    public bool IsDescending { get; }

    public OrderByClause(string propertyName, bool isDescending)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        IsDescending = isDescending;
    }
}
