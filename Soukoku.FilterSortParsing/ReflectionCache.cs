using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Soukoku.FilterSortParsing;

internal static class ReflectionCache
{
    private static readonly ConcurrentDictionary<string, MethodInfo> _queryableMethodCache = new();
    private static readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyPathCache = new();

    public static MethodInfo GetQueryableMethod(string methodName, Type sourceType, Type propertyType)
    {
        var cacheKey = methodName + ":" + sourceType.FullName + ":" + propertyType.FullName;
        return _queryableMethodCache.GetOrAdd(cacheKey, _ =>
        {
            var mi = typeof(System.Linq.Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == methodName && m.GetParameters().Length == 2);
            return mi.MakeGenericMethod(sourceType, propertyType);
        });
    }

    public static PropertyInfo[] GetPropertyPath(Type rootType, string propertyPath)
    {
        var cacheKey = rootType.FullName + ":" + propertyPath;
        return _propertyPathCache.GetOrAdd(cacheKey, _ =>
        {
            var parts = propertyPath.Split('.');
            var list = new System.Collections.Generic.List<PropertyInfo>(parts.Length);
            Type current = rootType;
            foreach (var part in parts)
            {
                var prop = current.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null)
                {
                    throw new ArgumentException($"Property '{part}' not found on type '{current.Name}'.");
                }
                list.Add(prop);
                current = prop.PropertyType;
            }
            return list.ToArray();
        });
    }
}