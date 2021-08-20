using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.EFCores;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace ShardingCore
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/16 15:18:37
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class ShardingConfigOption<TShardingDbContext, TActualDbContext> : IShardingConfigOption
    where TActualDbContext : DbContext, IShardingTableDbContext
    where TShardingDbContext : DbContext, IShardingTableDbContext<TActualDbContext>
    {
        private readonly Dictionary<Type, Type> _virtualRoutes = new Dictionary<Type, Type>();

        public Action<DbConnection, DbContextOptionsBuilder> ShardingDbContextOptionsCreator { get; set; }
        public void UseShardingDbContextOptions(Action<DbConnection, DbContextOptionsBuilder> shardingDbContextOptionsCreator)
        {
            ShardingDbContextOptionsCreator = shardingDbContextOptionsCreator ?? throw new ArgumentNullException(nameof(shardingDbContextOptionsCreator));
        }


        public Type ShardingDbContextType => typeof(TShardingDbContext);
        public Type ActualDbContextType => typeof(TActualDbContext);

        /// <summary>
        /// 添加分表路由
        /// </summary>
        /// <typeparam name="TRoute"></typeparam>
        public void AddShardingTableRoute<TRoute>() where TRoute : IVirtualTableRoute
        {
            var routeType = typeof(TRoute);
            //获取类型
            var genericVirtualRoute = routeType.GetInterfaces().FirstOrDefault(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IVirtualTableRoute<>)
                && it.GetGenericArguments().Any());
            if (genericVirtualRoute == null)
                throw new ArgumentException("add sharding route type error not assignable from IVirtualTableRoute<>.");

            var shardingEntityType = genericVirtualRoute.GetGenericArguments()[0];
            if (shardingEntityType == null)
                throw new ArgumentException("add sharding table route type error not assignable from IVirtualTableRoute<>");
            if (!_virtualRoutes.ContainsKey(shardingEntityType))
            {
                _virtualRoutes.Add(shardingEntityType, routeType);
            }
        }

        public Type GetVirtualRouteType(Type entityType)
        {
            if (!_virtualRoutes.ContainsKey(entityType))
                throw new ArgumentException($"{entityType} not found IVirtualTableRoute");
            return _virtualRoutes[entityType];
        }


        /// <summary>
        /// 如果数据库不存在就创建并且创建表除了分表的
        /// </summary>
        public bool EnsureCreatedWithOutShardingTable { get; set; }
        /// <summary>
        /// 是否需要在启动时创建分表
        /// </summary>
        public bool? CreateShardingTableOnStart { get; set; }
        /// <summary>
        /// 忽略建表时的错误
        /// </summary>
        public bool? IgnoreCreateTableError { get; set; }
    }
    
}

