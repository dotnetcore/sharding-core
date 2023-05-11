using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ReadWriteConfigurations;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/10/5 8:37:31
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class ShardingReadWriteExtension
    {
        /// <summary>
        /// 设置读写分离读取写数据库
        /// </summary>
        /// <param name="shardingDbContext"></param>
        /// <returns></returns>
        public static bool ReadWriteSeparationWriteOnly(this IShardingDbContext shardingDbContext)
        {
            shardingDbContext.SetReadWriteSeparation(false);
            return true;
        }

        /// <summary>
        /// 设置读写分离读读数据库
        /// </summary>
        /// <param name="shardingDbContext"></param>
        /// <returns></returns>
        public static bool ReadWriteSeparationReadOnly(this IShardingDbContext shardingDbContext)
        {
            shardingDbContext.SetReadWriteSeparation(true);
            return true;
        }

        /// <summary>
        /// 设置读写分离
        /// </summary>
        /// <param name="shardingDbContext"></param>
        /// <param name="readOnly">是否是读数据源</param>
        private static void SetReadWriteSeparation(this IShardingDbContext shardingDbContext, bool readOnly)
        {
            var shardingRuntimeContext = ((DbContext)shardingDbContext).GetShardingRuntimeContext();
            var shardingDbContextExecutor = shardingDbContext.GetShardingExecutor();
            var shardingReadWriteManager = shardingRuntimeContext.GetService<IShardingReadWriteManager>();
            var shardingReadWriteContext = shardingReadWriteManager.GetCurrent();
            if (shardingReadWriteContext != null)
            {
                if (shardingReadWriteContext.DefaultPriority > shardingDbContextExecutor.ReadWriteSeparationPriority)
                {
                    shardingDbContextExecutor.ReadWriteSeparationPriority=shardingReadWriteContext.DefaultPriority + 1;
                }
            }

            shardingDbContextExecutor.ReadWriteSeparationBehavior = ReadWriteDefaultEnableBehavior.DefaultEnable;
        }

        public static void SetReadWriteSeparation(this ShardingReadWriteContext shardingReadWriteContext, int priority,
            bool readOnly)
        {
            shardingReadWriteContext.DefaultPriority = priority;
            shardingReadWriteContext.DefaultReadEnable = readOnly;
        }

        /// <summary>
        /// 当前dbcontext是否是启用了读写分离
        /// </summary>
        /// <param name="shardingDbContext"></param>
        /// <returns></returns>
        [Obsolete(" plz use CurrentIsReadWriteSeparationBehavior")]
        public static bool CurrentIsReadWriteSeparation(this IShardingDbContext shardingDbContext)
        {
            if (shardingDbContext.IsUseReadWriteSeparation())
            {
                var shardingRuntimeContext = ((DbContext)shardingDbContext).GetShardingRuntimeContext();
                var shardingDbContextExecutor = shardingDbContext.GetShardingExecutor();
                var shardingReadWriteManager = shardingRuntimeContext.GetService<IShardingReadWriteManager>();
                var shardingReadWriteContext = shardingReadWriteManager.GetCurrent();
                if (shardingReadWriteContext != null)
                {
                    if (shardingReadWriteContext.DefaultPriority > shardingDbContextExecutor.ReadWriteSeparationPriority)
                    {
                        return shardingReadWriteContext.DefaultReadEnable;
                    }
                    else
                    {
                        return shardingDbContextExecutor.ReadWriteSeparation;
                    }
                }

                return shardingDbContextExecutor.ReadWriteSeparation;
            }

            return false;
        }
        public static ReadWriteDefaultEnableBehavior CurrentIsReadWriteSeparationBehavior(this IShardingDbContext shardingDbContext)
        {
            if (shardingDbContext.IsUseReadWriteSeparation())
            {
                var shardingRuntimeContext = ((DbContext)shardingDbContext).GetShardingRuntimeContext();
                var shardingDbContextExecutor = shardingDbContext.GetShardingExecutor();
                var shardingReadWriteManager = shardingRuntimeContext.GetService<IShardingReadWriteManager>();
                var shardingReadWriteContext = shardingReadWriteManager.GetCurrent();
                if (shardingReadWriteContext != null)
                {
                    if (shardingReadWriteContext.DefaultPriority > shardingDbContextExecutor.ReadWriteSeparationPriority)
                    {
                        return shardingReadWriteContext.DefaultEnableBehavior;
                    }
                    else
                    {
                        return shardingDbContextExecutor.ReadWriteSeparationBehavior;
                    }
                }

                return shardingDbContextExecutor.ReadWriteSeparationBehavior;
            }

            return ReadWriteDefaultEnableBehavior.DefaultDisable;
        }
    }
}