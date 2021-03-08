using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.VirtualTables;
using ShardingCore.DbContexts.ShardingDbContexts;
using ShardingCore.DbContexts.VirtualDbContexts;
using ShardingCore.Extensions;

namespace ShardingCore.DbContexts
{
/*
* @Author: xjm
* @Description:
* @Date: Thursday, 24 December 2020 08:22:48
* @Email: 326308290@qq.com
*/
    public class ShardingDbContextFactory:IShardingDbContextFactory
    {
        private readonly IShardingCoreOptions _shardingCoreOptions;
        private readonly IVirtualTableManager _virtualTableManager;

        public ShardingDbContextFactory(IShardingCoreOptions shardingCoreOptions,IVirtualTableManager virtualTableManager)
        {
            _shardingCoreOptions = shardingCoreOptions;
            _virtualTableManager = virtualTableManager;
        }
        public DbContext Create(string connectKey, ShardingDbContextOptions shardingDbContextOptions)
        {
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig(connectKey);
            return shardingConfigEntry.Creator(shardingDbContextOptions);
        }

        public DbContext Create(string connectKey, string tail,IDbContextOptionsProvider dbContextOptionsProvider)
        {
            var virtualTableConfigs = _virtualTableManager.GetAllVirtualTables(connectKey).GetVirtualTableDbContextConfigs();
            var shardingConfigEntry = _shardingCoreOptions.GetShardingConfig(connectKey);
            return shardingConfigEntry.Creator(new ShardingDbContextOptions(dbContextOptionsProvider.GetDbContextOptions(connectKey), tail, virtualTableConfigs));
        }
    }
}