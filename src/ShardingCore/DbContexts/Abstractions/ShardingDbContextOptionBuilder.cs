using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using ShardingCore.EFCores;

namespace ShardingCore.DbContexts.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 07 February 2021 15:25:36
* @Email: 326308290@qq.com
*/
    public class ShardingDbContextOptionBuilder
    {

        private readonly DbContextOptionsBuilder _builder;
        public ShardingDbContextOptionBuilder():this(new DbContextOptionsBuilder())
        {
            
        }
        public ShardingDbContextOptionBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            _builder = optionsBuilder
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>();
        }

        public ShardingDbContextOptionBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            _builder.UseLoggerFactory(loggerFactory);
            return this;
        }

        public DbContextOptionsBuilder GetOptionsBuilder()
        {
            return _builder;
        }
    }
    public class ShardingDbContextOptionBuilder<T>:ShardingDbContextOptionBuilder where T:DbContext
    {
    }
}