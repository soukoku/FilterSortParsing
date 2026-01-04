using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Soukoku.FilterSortParsing;

internal static class FilterApplier
{
    public static IQueryable<T> ApplyFilter<T>(IQueryable<T> source, FilterExpression? filterExpression)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (filterExpression == null)
        {
            return source;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var predicate = BuildExpression(filterExpression, parameter, typeof(T));
        var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);

        return source.Where(lambda);
    }

    private static Expression BuildExpression(FilterExpression filterExpression, ParameterExpression parameter, Type entityType)
    {
        switch (filterExpression.Type)
        {
            case FilterExpressionType.Comparison:
                return BuildComparisonExpression((ComparisonExpression)filterExpression, parameter, entityType);

            case FilterExpressionType.Logical:
                return BuildLogicalExpression((LogicalExpression)filterExpression, parameter, entityType);

            case FilterExpressionType.Not:
                return BuildNotExpression((NotExpression)filterExpression, parameter, entityType);

            default:
                throw new NotSupportedException($"Expression type {filterExpression.Type} is not supported.");
        }
    }

    private static Expression BuildComparisonExpression(ComparisonExpression comparison, ParameterExpression parameter, Type entityType)
    {
        // Get property expression
        Expression propertyAccess = parameter;
        var propertyInfos = ReflectionCache.GetPropertyPath(entityType, comparison.PropertyName);

        Type currentType = entityType;
        foreach (var property in propertyInfos)
        {
            propertyAccess = Expression.Property(propertyAccess, property);
            currentType = property.PropertyType;
        }

        Type propertyType = currentType;

        // Build comparison based on operator
        switch (comparison.Operator)
        {
            case "eq":
                return BuildEqualExpression(propertyAccess, comparison.Value, propertyType);

            case "ne":
                return Expression.Not(BuildEqualExpression(propertyAccess, comparison.Value, propertyType));

            case "gt":
                return BuildComparisonExpression(propertyAccess, comparison.Value, propertyType, Expression.GreaterThan);

            case "ge":
                return BuildComparisonExpression(propertyAccess, comparison.Value, propertyType, Expression.GreaterThanOrEqual);

            case "lt":
                return BuildComparisonExpression(propertyAccess, comparison.Value, propertyType, Expression.LessThan);

            case "le":
                return BuildComparisonExpression(propertyAccess, comparison.Value, propertyType, Expression.LessThanOrEqual);

            case "contains":
                return BuildStringMethodExpression(propertyAccess, comparison.Value, "Contains");

            case "startswith":
                return BuildStringMethodExpression(propertyAccess, comparison.Value, "StartsWith");

            case "endswith":
                return BuildStringMethodExpression(propertyAccess, comparison.Value, "EndsWith");

            default:
                throw new NotSupportedException($"Operator '{comparison.Operator}' is not supported.");
        }
    }

    private static Expression BuildEqualExpression(Expression propertyAccess, string value, Type propertyType)
    {
        var constantValue = ConvertValue(value, propertyType);
        var constant = Expression.Constant(constantValue, propertyType);
        return Expression.Equal(propertyAccess, constant);
    }

    private static Expression BuildComparisonExpression(Expression propertyAccess, string value, Type propertyType,
        Func<Expression, Expression, BinaryExpression> comparisonFactory)
    {
        var constantValue = ConvertValue(value, propertyType);
        var constant = Expression.Constant(constantValue, propertyType);
        return comparisonFactory(propertyAccess, constant);
    }

    private static Expression BuildStringMethodExpression(Expression propertyAccess, string value, string methodName)
    {
        if (propertyAccess.Type != typeof(string))
        {
            throw new InvalidOperationException($"String operation '{methodName}' can only be applied to string properties.");
        }

        var method = ReflectionCache.GetStringMethod(methodName);
        var constant = Expression.Constant(value, typeof(string));

        // Handle null check: property != null && property.Method(value)
        var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
        var methodCall = Expression.Call(propertyAccess, method, constant);
        return Expression.AndAlso(notNull, methodCall);
    }

    private static Expression BuildLogicalExpression(LogicalExpression logical, ParameterExpression parameter, Type entityType)
    {
        var left = BuildExpression(logical.Left, parameter, entityType);
        var right = BuildExpression(logical.Right, parameter, entityType);

        switch (logical.Operator)
        {
            case "and":
                return Expression.AndAlso(left, right);

            case "or":
                return Expression.OrElse(left, right);

            default:
                throw new NotSupportedException($"Logical operator '{logical.Operator}' is not supported.");
        }
    }

    private static Expression BuildNotExpression(NotExpression notExpr, ParameterExpression parameter, Type entityType)
    {
        var inner = BuildExpression(notExpr.Inner, parameter, entityType);
        return Expression.Not(inner);
    }

    private static object? ConvertValue(string value, Type targetType)
    {
        // Handle null
        if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
            {
                throw new InvalidOperationException($"Cannot assign null to non-nullable type {targetType.Name}.");
            }
            return null;
        }

        // Handle nullable types
        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Handle boolean
        if (underlyingType == typeof(bool))
        {
            if (bool.TryParse(value, out bool boolValue))
            {
                return boolValue;
            }
            throw new InvalidOperationException($"Cannot convert '{value}' to boolean.");
        }

        // Handle numeric types
        if (underlyingType == typeof(int))
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }
        if (underlyingType == typeof(long))
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }
        if (underlyingType == typeof(double))
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
        if (underlyingType == typeof(decimal))
        {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }
        if (underlyingType == typeof(float))
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }
        if (underlyingType == typeof(short))
        {
            return short.Parse(value, CultureInfo.InvariantCulture);
        }
        if (underlyingType == typeof(byte))
        {
            return byte.Parse(value, CultureInfo.InvariantCulture);
        }

        // Handle Guid
        if (underlyingType == typeof(Guid))
        {
            return Guid.Parse(value);
        }

        // Handle DateTime
        if (underlyingType == typeof(DateTime))
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture);
        }

        // Handle DateTimeOffset
        if (underlyingType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture);
        }

        // Handle string
        if (underlyingType == typeof(string))
        {
            return value;
        }

        // Try generic conversion
        try
        {
            return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot convert '{value}' to type {targetType.Name}.", ex);
        }
    }
}
