using System.Linq;

namespace FilterSortParsing;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> source, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return source;
        }
        return source;
    }


    public static IQueryable<T> ApplyOrderBy<T>(this IQueryable<T> source, string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }
        
        var clauses = OrderByParser.Parse(orderBy);
        return OrderByApplier.ApplyOrderBy(source, clauses);
    }
}
