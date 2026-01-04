using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Soukoku.FilterSortParsing;

internal static class ReflectionCache
{
    private static readonly ConcurrentDictionary<MethodCacheKey, MethodInfo> _queryableMethodCache = new();
    private static readonly ConcurrentDictionary<PropertyCacheKey, PropertyInfo[]> _propertyPathCache = new();

    public static MethodInfo GetQueryableMethod(string methodName, Type sourceType, Type propertyType)
    {
        var cacheKey = new MethodCacheKey(methodName, sourceType, propertyType);
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
        var cacheKey = new PropertyCacheKey(rootType, propertyPath);
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

    private readonly struct MethodCacheKey : IEquatable<MethodCacheKey>
    {
        private readonly string _methodName;
        private readonly Type _sourceType;
        private readonly Type _propertyType;

        public MethodCacheKey(string methodName, Type sourceType, Type propertyType)
        {
            _methodName = methodName;
            _sourceType = sourceType;
            _propertyType = propertyType;
        }

        public bool Equals(MethodCacheKey other)
        {
            return _methodName == other._methodName &&
                   _sourceType == other._sourceType &&
                   _propertyType == other._propertyType;
        }

        public override bool Equals(object? obj)
        {
            return obj is MethodCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (_methodName?.GetHashCode() ?? 0);
                hash = hash * 31 + (_sourceType?.GetHashCode() ?? 0);
                hash = hash * 31 + (_propertyType?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }

    private readonly struct PropertyCacheKey : IEquatable<PropertyCacheKey>
    {
        private readonly Type _rootType;
        private readonly string _propertyPath;

        public PropertyCacheKey(Type rootType, string propertyPath)
        {
            _rootType = rootType;
            _propertyPath = propertyPath;
        }

        public bool Equals(PropertyCacheKey other)
        {
            return _rootType == other._rootType &&
                   _propertyPath == other._propertyPath;
        }

        public override bool Equals(object? obj)
        {
            return obj is PropertyCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (_rootType?.GetHashCode() ?? 0);
                hash = hash * 31 + (_propertyPath?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}