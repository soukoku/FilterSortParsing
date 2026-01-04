using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Soukoku.FilterSortParsing;

internal static class ReflectionCache
{
    private static readonly ConcurrentDictionary<MethodCacheKey, MethodInfo> _queryableMethodCache = new();
    private static readonly ConcurrentDictionary<PropertyCacheKey, PropertyInfo[]> _propertyPathCache = new();
    
    // Cache common string method lookups
    private static readonly MethodInfo _containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
    private static readonly MethodInfo _startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;
    private static readonly MethodInfo _endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;

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

    public static MethodInfo GetStringMethod(string methodName)
    {
        return methodName switch
        {
            "Contains" => _containsMethod,
            "StartsWith" => _startsWithMethod,
            "EndsWith" => _endsWithMethod,
            _ => throw new ArgumentException($"Unknown string method: {methodName}")
        };
    }

    public static PropertyInfo[] GetPropertyPath(Type rootType, string propertyPath)
    {
        var cacheKey = new PropertyCacheKey(rootType, propertyPath);
        return _propertyPathCache.GetOrAdd(cacheKey, _ =>
        {
            // Fast path: no dots (most common case)
            int firstDot = propertyPath.IndexOf('.');
            if (firstDot == -1)
            {
                var prop = rootType.GetProperty(propertyPath, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null)
                {
                    throw new ArgumentException($"Property '{propertyPath}' not found on type '{rootType.Name}'.");
                }
                return new[] { prop };
            }

            // Nested path: use span-based iteration to avoid Split allocation
            var list = new List<PropertyInfo>(2);
            Type current = rootType;
            ReadOnlySpan<char> remaining = propertyPath.AsSpan();
            
            while (!remaining.IsEmpty)
            {
                int dotIndex = remaining.IndexOf('.');
                ReadOnlySpan<char> part = dotIndex >= 0 ? remaining.Slice(0, dotIndex) : remaining;
                
                // GetProperty requires string, so allocate only the segment
                var prop = current.GetProperty(part.ToString(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null)
                {
                    throw new ArgumentException($"Property '{part.ToString()}' not found on type '{current.Name}'.");
                }
                
                list.Add(prop);
                current = prop.PropertyType;
                remaining = dotIndex >= 0 ? remaining.Slice(dotIndex + 1) : ReadOnlySpan<char>.Empty;
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