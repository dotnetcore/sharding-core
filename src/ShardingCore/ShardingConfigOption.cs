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
using ShardingCore.Sharding.ReadWriteConfigurations;

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
        where TShardingDbContext : DbContext, IShardingDbContext<TActualDbContext>
    {
        private readonly Dictionary<Type, Type> _virtualRoutes = new Dictionary<Type, Type>();

        public Action<DbConnection, DbContextOptionsBuilder> SameConnectionConfigure { get;private set; }
        public Action<string,DbContextOptionsBuilder> DefaultQueryConfigure { get; private set; }
        /// <summary>
        /// 配置数据库分表查询和保存时的DbContext创建方式
        /// </summary>
        /// <param name="sameConnectionConfigure">DbConnection下如何配置因为不同的DbContext支持事务需要使用同一个DbConnection</param>
        /// <param name="defaultQueryConfigure">默认查询DbContext创建的配置</param>

        public void UseShardingOptionsBuilder(Action<DbConnection, DbContextOptionsBuilder> sameConnectionConfigure, Action<string,DbContextOptionsBuilder> defaultQueryConfigure = null)
        {
            SameConnectionConfigure = sameConnectionConfigure ?? throw new ArgumentNullException(nameof(sameConnectionConfigure));
            DefaultQueryConfigure = defaultQueryConfigure ?? throw new ArgumentNullException(nameof(defaultQueryConfigure));
        }

        public bool UseReadWrite => ReadConnStringConfigure != null;
        public Func<IServiceProvider, IEnumerable<string>> ReadConnStringConfigure { get; private set; }
        public ReadStrategyEnum ReadStrategyEnum { get; private set; }
        public bool ReadWriteDefaultEnable { get; private set; }
        public int ReadWriteDefaultPriority { get; private set; }
        public ReadConnStringGetStrategyEnum ReadConnStringGetStrategy { get; private set; }

        /// <summary>
        /// 使用读写分离配置
        /// </summary>
        /// <param name="readConnStringConfigure"></param>
        /// <param name="readStrategyEnum"></param>
        /// <param name="defaultEnable">考虑到很多时候读写分离的延迟需要马上用到写入的数据所以默认关闭需要的话自己开启或者通过IShardingReadWriteManager,false表示默认不走读写分离除非你自己开启,true表示默认走读写分离除非你禁用,</param>
        /// <param name="defaultPriority">IShardingReadWriteManager.CreateScope()会判断dbcontext的priority然后判断是否启用readwrite</param>
        /// <param name="readConnStringGetStrategy">读写分离可能会造成每次查询不一样甚至分表后的分页会有错位问题，因为他不是一个原子操作,所以如果整个请求为一次读写切换大多数更加合适</param>
        public void UseReadWriteConfiguration(Func<IServiceProvider, IEnumerable<string>> readConnStringConfigure, ReadStrategyEnum readStrategyEnum,bool defaultEnable=false,int defaultPriority=10,ReadConnStringGetStrategyEnum readConnStringGetStrategy= ReadConnStringGetStrategyEnum.LatestFirstTime)
        {
            ReadConnStringConfigure = readConnStringConfigure ?? throw new ArgumentNullException(nameof(readConnStringConfigure));
            ReadStrategyEnum = readStrategyEnum;
            ReadWriteDefaultEnable = defaultEnable;
            ReadWriteDefaultPriority = defaultPriority;
            ReadConnStringGetStrategy = readConnStringGetStrategy;
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