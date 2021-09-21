//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using ShardingCore.Core;
//using ShardingCore.Core.Internal.Visitors;
//using ShardingCore.Core.Internal.Visitors.Querys;
//using ShardingCore.Core.VirtualRoutes;
//using ShardingCore.Core.VirtualTables;

//namespace ShardingCore.Utils
//{
///*
//* @Author: xjm
//* @Description:
//* @Date: Saturday, 19 December 2020 20:20:29
//* @Email: 326308290@qq.com
//*/
//    public class ShardingKeyUtil
//    {
//        private static readonly ConcurrentDictionary<Type, ShardingTableConfig> _caches = new ConcurrentDictionary<Type, ShardingTableConfig>();

//        private ShardingKeyUtil()
//        {
//        }

//        public static ShardingTableConfig Parse(Type entityType)
//        {
//            if (!typeof(IShardingTable).IsAssignableFrom(entityType))
//                throw new NotSupportedException(entityType.ToString());
//            if (_caches.TryGetValue(entityType, out var shardingEntityConfig))
//            {
//                return shardingEntityConfig;
//            }

//            PropertyInfo[] shardingProperties = entityType.GetProperties();
//            foreach (var shardingProperty in shardingProperties)
//            {
//                var attribbutes = shardingProperty.GetCustomAttributes(true);
//                if (attribbutes.FirstOrDefault(x => x.GetType() == typeof(ShardingTableKeyAttribute)) is ShardingTableKeyAttribute shardingKeyAttribute)
//                {
//                    if (shardingEntityConfig != null)
//                        throw new ArgumentException($"{entityType} found more than one [ShardingKeyAttribute]");
//                    shardingEntityConfig = new ShardingTableConfig()
//                    {
//                        ShardingEntityType = entityType,
//                        ShardingField = shardingProperty.Name,
//                        AutoCreateTable = shardingKeyAttribute.AutoCreateTableOnStart==ShardingKeyAutoCreateTableEnum.UnKnown?(bool?)null:(shardingKeyAttribute.AutoCreateTableOnStart==ShardingKeyAutoCreateTableEnum.Create),
//                        TailPrefix = shardingKeyAttribute.TailPrefix
//                    };
//                    _caches.TryAdd(entityType, shardingEntityConfig);
//                }
//            }

//            return shardingEntityConfig;
//        }



//        //public static ISet<Type> GetShardingEntitiesFilter(IQueryable queryable)
//        //{
//        //    ShardingEntitiesVisitor visitor = new ShardingEntitiesVisitor();

//        //    visitor.Visit(queryable.Expression);

//        //    return visitor.GetShardingEntities();
//        //}

//    }
//}