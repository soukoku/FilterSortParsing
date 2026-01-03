using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Soukoku.FilterSortParsing;

internal static class OrderByApplier
{
    public static IQueryable<T> ApplyOrderBy<T>(IQueryable<T> source, List<OrderByClause> clauses)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (clauses == null || clauses.Count == 0)
        {
            return source;
        }

        IOrderedQueryable<T>? orderedQuery = null;

        for (int i = 0; i < clauses.Count; i++)
        {
            var clause = clauses[i];

            if (orderedQuery == null)
            {
                orderedQuery = clause.IsDescending
                    ? ApplyOrderByDescending(source, clause.PropertyName)
                    : ApplyOrderByAscending(source, clause.PropertyName);
            }
            else
            {
                orderedQuery = clause.IsDescending
                    ? ApplyThenByDescending(orderedQuery, clause.PropertyName)
                    : ApplyThenByAscending(orderedQuery, clause.PropertyName);
            }
        }

        return orderedQuery ?? source;
    }

    private static IOrderedQueryable<T>? ApplyOrderByAscending<T>(IQueryable<T> source, string propertyName)
    {
        var expression = CreatePropertyExpression<T>(propertyName);
        return CallOrderMethod(source, "OrderBy", expression);
    }

    private static IOrderedQueryable<T>? ApplyOrderByDescending<T>(IQueryable<T> source, string propertyName)
    {
        var expression = CreatePropertyExpression<T>(propertyName);
        return CallOrderMethod(source, "OrderByDescending", expression);
    }

    private static IOrderedQueryable<T>? ApplyThenByAscending<T>(IOrderedQueryable<T> source, string propertyName)
    {
        var expression = CreatePropertyExpression<T>(propertyName);
        return CallOrderMethod(source, "ThenBy", expression);
    }

    private static IOrderedQueryable<T>? ApplyThenByDescending<T>(IOrderedQueryable<T> source, string propertyName)
    {
        var expression = CreatePropertyExpression<T>(propertyName);
        return CallOrderMethod(source, "ThenByDescending", expression);
    }

    private static LambdaExpression CreatePropertyExpression<T>(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression propertyAccess = parameter;

        var propertyInfos = ReflectionCache.GetPropertyPath(typeof(T), propertyName);

        foreach (var property in propertyInfos)
        {
            propertyAccess = Expression.Property(propertyAccess, property);
        }

        return Expression.Lambda(propertyAccess, parameter);
    }

    private static IOrderedQueryable<T>? CallOrderMethod<T>(IQueryable<T> source, string methodName, LambdaExpression keySelector)
    {
        var sourceType = typeof(T);
        var propertyType = keySelector.ReturnType;

        var method = ReflectionCache.GetQueryableMethod(methodName, sourceType, propertyType);

        var result = method.Invoke(null, new object[] { source, keySelector });
        return (IOrderedQueryable<T>?)result;
    }
}
