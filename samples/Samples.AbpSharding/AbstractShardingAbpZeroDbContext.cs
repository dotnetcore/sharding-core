// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.Domain.Entities;
using Abp.EntityFrameworkCore;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.EFCores;
using ShardingCore.Extensions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.ShardingDbContextExecutors;

namespace Samples.AbpSharding
{
    public abstract class AbstractShardingAbpZeroDbContext<TTenant, TRole, TUser, TSelf>
        : AbpZeroDbContext<TTenant, TRole, TUser, TSelf>,
        IShardingDbContext,IShardingTableDbContext
        where TTenant : AbpTenant<TUser>
        where TRole : AbpRole<TUser>
        where TUser : AbpUser<TUser>
        where TSelf : AbpZeroDbContext<TTenant, TRole, TUser, TSelf>
    {

        protected AbstractShardingAbpZeroDbContext(DbContextOptions<TSelf> options)
            : base(options)
        {
        }
        public IRouteTail RouteTail { get; set; }

        #region Sharding Core 方法实现

        private bool _createExecutor = false;

        private  IShardingDbContextExecutor _shardingDbContextExecutor;
        public IShardingDbContextExecutor GetShardingExecutor()
        {
            if (!_createExecutor)
            {
                _shardingDbContextExecutor=this.DoCreateShardingDbContextExecutor();
                _createExecutor = true;
            }
            return _shardingDbContextExecutor;
        }
        private IShardingDbContextExecutor DoCreateShardingDbContextExecutor()
        {
            var shardingDbContextExecutor = this.CreateShardingDbContextExecutor();
            if (shardingDbContextExecutor != null)
            {
                shardingDbContextExecutor.EntityCreateDbContextBefore += (sender, args) =>
                {
                    CheckAndSetShardingKeyThatSupportAutoCreate(args.Entity);
                };
                shardingDbContextExecutor.CreateDbContextAfter += (sender, args) =>
                {
                    var argsDbContext = args.DbContext;
                    FillDbContextInject(argsDbContext);
                };
            }
            
            return shardingDbContextExecutor;
        }

        #endregion


        #region 内部方法

        /// <summary>
        /// 检查并设置分表键值是否支持自动创建
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        protected virtual void CheckAndSetShardingKeyThatSupportAutoCreate<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is IShardingKeyIsGuId)
            {

                if (entity is IEntity<Guid> guidEntity)
                {
                    if (guidEntity.Id != default)
                    {
                        return;
                    }
                    var idProperty = entity.GetObjectProperty(nameof(IEntity<Guid>.Id));

                    var dbGeneratedAttr = idProperty.GetCustomAttributes(false)
                        .Where(o => o.GetType() == typeof(DatabaseGeneratedAttribute))
                        .FirstOrDefault() as DatabaseGeneratedAttribute;

                    //var dbGeneratedAttr = ReflectionHelper
                    //    .GetSingleAttributeOrDefault<DatabaseGeneratedAttribute>(
                    //        idProperty
                    //    );

                    if (dbGeneratedAttr != null && dbGeneratedAttr.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                    {
                        return;
                    }

                    guidEntity.Id = GuidGenerator.Create();
                }
            }
            else if (entity is IShardingKeyIsCreationTime)
            {
                this.SetCreationAuditProperties(entity, this.GetAuditUserId());

                //AuditPropertySetter?.SetCreationProperties(entity);
            }
        }

        /// <summary>
        /// 填充DbContext需要的依赖项
        /// </summary>
        /// <param name="dbContext"></param>
        protected virtual void FillDbContextInject(DbContext dbContext)
        {
            if ( dbContext is AbpZeroCommonDbContext<TRole, TUser, TSelf> abpDbContext)
            {
                // AbpZeroCommonDbContext
                if (abpDbContext.EntityHistoryHelper == null)
                {
                    abpDbContext.EntityHistoryHelper = this.EntityHistoryHelper;
                }

                // AbpDbContext
                if (abpDbContext.AbpSession == null)
                {
                    abpDbContext.AbpSession = this.AbpSession;
                }
                if (abpDbContext.EntityChangeEventHelper == null)
                {
                    abpDbContext.EntityChangeEventHelper = this.EntityChangeEventHelper;
                }
                if (abpDbContext.Logger == null)
                {
                    abpDbContext.Logger = this.Logger;
                }
                if (abpDbContext.EventBus == null)
                {
                    abpDbContext.EventBus = this.EventBus;
                }
                if (abpDbContext.GuidGenerator == null)
                {
                    abpDbContext.GuidGenerator = this.GuidGenerator;
                }
                if (abpDbContext.CurrentUnitOfWorkProvider == null)
                {
                    abpDbContext.CurrentUnitOfWorkProvider = this.CurrentUnitOfWorkProvider;
                }
                if (abpDbContext.MultiTenancyConfig == null)
                {
                    abpDbContext.MultiTenancyConfig = this.MultiTenancyConfig;
                }
                abpDbContext.SuppressAutoSetTenantId = this.SuppressAutoSetTenantId;
            }
        }

        #endregion

        public override void Dispose()
        {

            _shardingDbContextExecutor?.Dispose();
            base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            if(_shardingDbContextExecutor!=null)
            {
                await _shardingDbContextExecutor.DisposeAsync();
            }

            await base.DisposeAsync();
        }

    }
}
