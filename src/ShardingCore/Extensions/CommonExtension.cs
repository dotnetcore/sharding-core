using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 01 January 2021 19:51:44
* @Email: 326308290@qq.com
*/
    public static class CommonExtension
    {

        /// <summary>
        /// IShardingDbContext
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static bool IsShardingDbContext(this DbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));
            return dbContext is IShardingDbContext;
        }
        /// <summary>
        /// IShardingDbContext
        /// </summary>
        /// <param name="dbContextType"></param>
        /// <returns></returns>
        public static bool IsShardingDbContext(this Type dbContextType)
        {
            if (dbContextType == null)
                throw new ArgumentNullException(nameof(dbContextType));
            if (!typeof(DbContext).IsAssignableFrom(dbContextType))
                return false;
            return typeof(IShardingDbContext).IsAssignableFrom(dbContextType);
        }

        /// <summary>
        /// IShardingDbContext
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static bool IsShardingTableDbContext(this DbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));
            return dbContext is IShardingTableDbContext;
        }
        /// <summary>
        /// IShardingDbContext
        /// </summary>
        /// <param name="dbContextType"></param>
        /// <returns></returns>
        public static bool IsShardingTableDbContext(this Type dbContextType)
        {
            if (dbContextType == null)
                throw new ArgumentNullException(nameof(dbContextType));
            if (!typeof(DbContext).IsAssignableFrom(dbContextType))
                return false;
            return typeof(IShardingTableDbContext).IsAssignableFrom(dbContextType);
        }
        // /// <summary>
        // /// 虚拟表转换成对应的db配置
        // /// </summary>
        // /// <param name="virtualTables"></param>
        // /// <returns></returns>
        // public static List<VirtualTableDbContextConfig> GetVirtualTableDbContextConfigs(this List<IVirtualTable> virtualTables)
        // {
        //     return virtualTables.Select(o => new VirtualTableDbContextConfig(o.EntityType, o.GetVirtualTableName(), o.ShardingConfigOption.TableSeparator)).ToList();
        // }
        /// <summary>
        /// 是否是集合contains方法
        /// </summary>
        /// <param name="express"></param>
        /// <returns></returns>
        public static bool IsEnumerableContains(this MethodCallExpression express)
        {
            var methodName = express.Method.Name;
            return methodName == nameof(IList.Contains)&& (express.Method.DeclaringType?.Namespace.IsInEnumerable()??false);
        }
        public static bool IsStringContains(this MethodCallExpression express)
        {
            var methodName = express.Method.Name;
            return methodName == nameof(string.Contains)&& (express.Method.DeclaringType==typeof(string));
        }
        public static bool IsStringStartWith(this MethodCallExpression express)
        {
            var methodName = express.Method.Name;
            return methodName == nameof(string.StartsWith)&& (express.Method.DeclaringType==typeof(string));
        }
        public static bool IsStringEndWith(this MethodCallExpression express)
        {
            var methodName = express.Method.Name;
            return methodName == nameof(string.EndsWith)&& (express.Method.DeclaringType==typeof(string));
        }
        /// <summary>
        /// 是否是equal方法
        /// </summary>
        /// <param name="express"></param>
        /// <returns></returns>
        public static bool IsNamedEquals(this MethodCallExpression express)
        {
            return nameof(object.Equals).Equals(express.Method.Name);
        }
        //public static bool IsNamedCompareOrdinal(this BinaryExpression express)
        //{
        //    express.
        //    return nameof(string.CompareOrdinal).Equals(express.Method.Name) || nameof(string.CompareTo).Equals(express.Method.Name) || nameof(string.Compare).Equals(express.Method.Name);
        //}
        public static bool IsNamedComparison(this BinaryExpression express,out MethodCallExpression methodCallExpression)
        {
            if (express.Left is MethodCallExpression m1 && m1.IsNamedComparison())
            {
                methodCallExpression = m1;
                return true;
            }
            if (express.Right is MethodCallExpression m2 && m2.IsNamedComparison())
            {
                methodCallExpression = m2;
                return true;
            }

            methodCallExpression = null;
            return false;
        }
        public static bool GetComparisonLeftAndRight(this MethodCallExpression methodCallExpression, out (Expression Left,Expression Right) comparisonValue)
        {

            if (methodCallExpression.IsNamedCompare())
            {
                if (methodCallExpression.Arguments.Count == 2)
                {
                    comparisonValue = (methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
                    return true;
                }
            }

            if (methodCallExpression.IsNamedCompareTo())
            {
                if (methodCallExpression.Arguments.Count == 1 && methodCallExpression.Object != null)
                {
                    comparisonValue = (methodCallExpression.Object, methodCallExpression.Arguments[0]);
                    return true;
                }
            }
            comparisonValue = (null,null);
            return false;
        }
        public static bool IsNamedComparison(this MethodCallExpression express)
        {
            return express.IsNamedCompare() || express.IsNamedCompareTo();
        }
        public static bool IsNamedCompare(this MethodCallExpression express)
        {
            return nameof(string.CompareTo).Equals(express.Method.Name) || nameof(string.Compare).Equals(express.Method.Name);
        }
        public static bool IsNamedCompareTo(this MethodCallExpression express)
        {
            return nameof(string.CompareTo).Equals(express.Method.Name) || nameof(string.Compare).Equals(express.Method.Name);
        }

        //public static ISet<Type> ParseQueryableEntities(this IQueryable queryable, Type dbContextType)
        //{
        //    return ShardingUtil.GetQueryEntitiesFilter(queryable, dbContextType);
        //}

        public static bool IsMemberQueryable(this MemberExpression memberExpression)
        {
            if (memberExpression == null)
                throw new ArgumentNullException(nameof(memberExpression));
            return (memberExpression.Type.FullName?.StartsWith("System.Linq.IQueryable`1") ?? false) || typeof(DbContext).IsAssignableFrom(memberExpression.Type);
        }

        public static Type GetSequenceType(this Type type)
        {
            var sequenceType = TryGetSequenceType(type);
            if (sequenceType == null)
            {
                // TODO: Add exception message
                throw new ArgumentException();
            }

            return sequenceType;
        }

        public static Type TryGetSequenceType(this Type type)
            => type.TryGetElementType(typeof(IEnumerable<>))
               ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));

        public static Type TryGetElementType(this Type type, Type interfaceOrBaseType)
        {
            if (type.IsGenericTypeDefinition)
            {
                return null;
            }

            var types = GetGenericTypeImplementations(type, interfaceOrBaseType);

            Type singleImplementation = null;
            foreach (var implementation in types)
            {
                if (singleImplementation == null)
                {
                    singleImplementation = implementation;
                }
                else
                {
                    singleImplementation = null;
                    break;
                }
            }

            return singleImplementation?.GenericTypeArguments.FirstOrDefault();
        }
        public static IEnumerable<Type> GetGenericTypeImplementations(this Type type, Type interfaceOrBaseType)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                var baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                    ? typeInfo.ImplementedInterfaces
                    : type.GetBaseTypes();
                foreach (var baseType in baseTypes)
                {
                    if (baseType.IsGenericType
                        && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                    {
                        yield return baseType;
                    }
                }

                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return type;
                }
            }
        }
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            type = type.BaseType;

            while (type != null)
            {
                yield return type;

                type = type.BaseType;
            }
        }
        
        public static bool IsMethodReturnTypeQueryableType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            return typeof(IQueryable).IsAssignableFrom(type);
        }
    }
}