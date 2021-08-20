using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Utils;

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
        /// IShardingTableDbContext
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
        /// IShardingTableDbContext
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
        /// <summary>
        /// 是否基继承至IShardingTable
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool IsShardingTable(this Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            return typeof(IShardingTable).IsAssignableFrom(entityType);
        }

        /// <summary>
        /// 是否基继承至IShardingTable
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsShardingTable(this object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            return entity is IShardingTable;
        }
        // /// <summary>
        // /// 虚拟表转换成对应的db配置
        // /// </summary>
        // /// <param name="virtualTables"></param>
        // /// <returns></returns>
        // public static List<VirtualTableDbContextConfig> GetVirtualTableDbContextConfigs(this List<IVirtualTable> virtualTables)
        // {
        //     return virtualTables.Select(o => new VirtualTableDbContextConfig(o.EntityType, o.GetOriginalTableName(), o.ShardingConfigOption.TailPrefix)).ToList();
        // }
        /// <summary>
        /// 是否是集合contains方法
        /// </summary>
        /// <param name="express"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static bool IsEnumerableContains(this MethodCallExpression express, string methodName)
        {
            return  express.Method.DeclaringType.Namespace.IsIn("System.Linq", "System.Collections.Generic") && methodName == nameof(IList.Contains);
        }

        public static ISet<Type> ParseQueryableRoute(this IQueryable queryable)
        {
            return ShardingKeyUtil.GetQueryEntitiesFilter(queryable);
        }

        public static T IfDo<T>(this T t, bool @if,Func<T,T> build)
        {
            if (@if)
            {
                return build(t);
            }

            return t;
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
    }
}