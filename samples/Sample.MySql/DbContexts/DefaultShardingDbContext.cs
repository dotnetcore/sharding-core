using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sample.MySql.Domain.Entities;
using Sample.MySql.Domain.Maps;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace Sample.MySql.DbContexts
{
    public class DefaultShardingDbContext:AbstractShardingDbContext, IShardingTableDbContext
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
            //切记不要在构造函数中使用会让模型提前创建的方法
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            //Database.SetCommandTimeout(30000);
        }
        private readonly MethodInfo? _configureGlobalFiltersMethodInfo =
            typeof(DefaultShardingDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
            modelBuilder.ApplyConfiguration(new SysUserLogByMonthMap());
            // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            // {
            //     _configureGlobalFiltersMethodInfo?.MakeGenericMethod(entityType.ClrType)
            //         .Invoke(this, new object[] { modelBuilder, entityType });
            // }

            modelBuilder.Entity<SysUserLogByMonth>().HasData(new SysUserLogByMonth() { Id = "1", Time = DateTime.Now });
            modelBuilder.Entity<SysTest>().HasData(new SysTest() { Id = "1", UserId = "123" });
        }

        
        protected void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType)
            where TEntity : class
        {
            var filterExpression = CreateFilterExpression<TEntity>();

            if (filterExpression != null) modelBuilder.Entity<TEntity>().HasQueryFilter(filterExpression);
        }

        protected Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>() where TEntity : class
        {
            Expression<Func<TEntity, bool>>? expression = null;
            if (typeof(TEntity) == typeof(SysTest))
            {  
                expression = e => ((IUser)e).UserId == "123";
              
            }  if (typeof(TEntity) == typeof(SysUserMod))
            {  
                expression = e => ((IAge)e).Age == 99;
              
            }

            return expression;
        }
        public IRouteTail RouteTail { get; set; }
    }
}
