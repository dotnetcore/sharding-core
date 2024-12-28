﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Extensions;
using ShardingCore.Extensions.ShardingQueryableExtensions;
using ShardingCore.Sharding.Abstractions;
using ShardingCore.Sharding.Visitors.ShardingExtractParameters;

namespace ShardingCore.Sharding.Parsers.Visitors
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/5/1 21:43:12
    /// Email: 326308290@qq.com
    internal class ShardingQueryPrepareVisitor : ExpressionVisitor
    {
        private bool isNotSupport;
        private ShardingQueryableUseConnectionModeOptions shardingQueryableUseConnectionModeOptions;
        private ShardingQueryableAsRouteOptions shardingQueryableAsRouteOptions;
        private ShardingQueryableReadWriteSeparationOptions shardingQueryableReadWriteSeparationOptions;
        private ShardingQueryableAsSequenceOptions shardingQueryableAsSequenceOptions;



        private readonly ITrackerManager _trackerManager;
        private bool? isNoTracking;
        private bool isIgnoreFilter;
        private readonly Dictionary<Type, IQueryable> shardingEntities = new();

        public ShardingQueryPrepareVisitor(IShardingDbContext shardingDbContext)
        {
            _trackerManager = ((DbContext)shardingDbContext).GetShardingRuntimeContext()
                .GetTrackerManager();
        }
        public ShardingQueryPrepareVisitor(ITrackerManager trackerManager)
        {
            _trackerManager = trackerManager;
        }
        public ShardingPrepareResult GetShardingPrepareResult()
        {
            return new ShardingPrepareResult(isNotSupport,
                shardingQueryableAsRouteOptions,
                shardingQueryableUseConnectionModeOptions,
                shardingQueryableReadWriteSeparationOptions,
                shardingQueryableAsSequenceOptions,
                shardingEntities, isNoTracking, isIgnoreFilter);
        }
#if EFCORE2 || EFCORE3
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable)
            {
                TryAddShardingEntities(queryable.ElementType, null);
            }

            return base.VisitConstant(node);
        }
#endif
#if EFCORE5 || EFCORE6 || EFCORE7 || EFCORE8 || EFCORE9
        protected override Expression VisitExtension(Expression node)
        {
            if (node is QueryRootExpression queryRootExpression)
            {
#if EFCORE7 || EFCORE8 || EFCORE9
                TryAddShardingEntities(queryRootExpression.ElementType, null);
#else
                TryAddShardingEntities(queryRootExpression.EntityType.ClrType, null);
#endif
            }
            return base.VisitExtension(node);
        }
#endif

                private void TryAddShardingEntities(Type entityType, IQueryable queryable)
        {
            if (!shardingEntities.ContainsKey(entityType))
            {
                shardingEntities.Add(entityType, queryable);
            }
        }

        protected override Expression VisitMember
            (MemberExpression memberExpression)
        {

            //if (memberExpression.IsMemberQueryable()) //2x,3x 路由 单元测试 分表和不分表
            //{
            // Recurse down to see if we can simplify...
            var expression = Visit(memberExpression.Expression);

            // If we've ended up with a constant, and it's a property or a field,
            // we can simplify ourselves to a constant
            if (expression is ConstantExpression)
            {
                object container = ((ConstantExpression)expression).Value;
                var member = memberExpression.Member;
                if (member is FieldInfo fieldInfo)
                {
                    object value = fieldInfo.GetValue(container);
                    if (value is IQueryable queryable)
                    {
                        var shardingQueryPrepareVisitor = new ShardingQueryPrepareVisitor(_trackerManager);
                        shardingQueryPrepareVisitor.Visit(queryable.Expression);
                        var shardingPrepareResult = shardingQueryPrepareVisitor.GetShardingPrepareResult();
                        foreach (var keyValuePair in shardingPrepareResult.QueryEntities)
                        {
                            TryAddShardingEntities(keyValuePair.Key, keyValuePair.Value??queryable);
                        }
                    }
                    //return Expression.Constant(value);
                }
                if (member is PropertyInfo propertyInfo)
                {
                    object value = propertyInfo.GetValue(container, null);
                    if (value is IQueryable queryable)
                    {
                        TryAddShardingEntities(queryable.ElementType, queryable);
                    }
                }
            }
            //}
            return base.VisitMember(memberExpression);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {

            switch (node.Method.Name)
            {
                case nameof(EntityFrameworkQueryableExtensions.AsNoTracking): isNoTracking = true; break;
                case nameof(EntityFrameworkQueryableExtensions.AsTracking): isNoTracking = false; break;
                case nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters): isIgnoreFilter = true; break;
                // case nameof(EntityFrameworkQueryableExtensions.Include):
                // case nameof(EntityFrameworkQueryableExtensions.ThenInclude): DiscoverQueryEntities(node); break;
                default:
                    {
                        if (node.Method.ReturnType.IsMethodReturnTypeQueryableType() && node.Method.ReturnType.IsGenericType)
                        {
                            DiscoverQueryEntities(node);
                        }
                        var customerExpression = DiscoverCustomerQueryEntities(node);
                        if (customerExpression != null)
                        {
                            return Visit(customerExpression);
                        }
                    }
                    ; break;
            }


            return base.VisitMethodCall(node);
        }

        private Expression DiscoverCustomerQueryEntities(MethodCallExpression node)
        {

            if (node.Method.IsGenericMethod)
            {
                var genericMethodDefinition = node.Method.GetGenericMethodDefinition();

                // find  notsupport extention calls
                if (genericMethodDefinition == EntityFrameworkShardingQueryableExtension.NotSupportMethodInfo)
                {
                    isNotSupport = true;
                    // cut out extension expression
                    return node.Arguments[0];
                }
                else if (genericMethodDefinition == EntityFrameworkShardingQueryableExtension.UseConnectionModeMethodInfo)
                {
                    shardingQueryableUseConnectionModeOptions = node.Arguments
                        .OfType<ConstantExpression>()
                        .Where(o => o.Value is ShardingQueryableUseConnectionModeOptions)
                        .Select(o => (ShardingQueryableUseConnectionModeOptions)o.Value)
                        .Last();
                    return node.Arguments[0];
                }
                else if (genericMethodDefinition == EntityFrameworkShardingQueryableExtension.AsRouteMethodInfo)
                {
                    shardingQueryableAsRouteOptions = node.Arguments
                        .OfType<ConstantExpression>()
                        .Where(o => o.Value is ShardingQueryableAsRouteOptions)
                        .Select(o => (ShardingQueryableAsRouteOptions)o.Value)
                        .Last();
                    return node.Arguments[0];
                }
                else if (genericMethodDefinition == EntityFrameworkShardingQueryableExtension.AsSequenceModeMethodInfo)
                {
                    shardingQueryableAsSequenceOptions = node.Arguments
                        .OfType<ConstantExpression>()
                        .Where(o => o.Value is ShardingQueryableAsSequenceOptions)
                        .Select(o => (ShardingQueryableAsSequenceOptions)o.Value)
                        .Last();
                    return node.Arguments[0];
                }
                else
                {
                    for (var i = 0; i < node.Arguments.Count; i++)
                    {
                        var nodeArgument = node.Arguments[i];
                        if (typeof(IQueryable).IsAssignableFrom(nodeArgument.Type))
                        {
                            Visit(nodeArgument);
                        }
                    }
                }
            }

            return null;
        }

        private void DiscoverQueryEntities(MethodCallExpression node)
        {
            var genericArguments = node.Type.GetGenericArguments();
            for (var i = 0; i < genericArguments.Length; i++)
            {
                var genericArgument = genericArguments[i];

                if (typeof(IEnumerable).IsAssignableFrom(genericArgument))
                {
                    var arguments = genericArgument.GetGenericArguments();
                    foreach (var argument in arguments)
                    {
                        //if is db context model
                        if (_trackerManager.IsDbContextModel(argument))
                        {
                            TryAddShardingEntities(argument, null);
                        }
                    }
                }

                if (!genericArgument.IsSimpleType())
                {
                    //if is db context model
                    if (_trackerManager.IsDbContextModel(genericArgument))
                    {
                        TryAddShardingEntities(genericArgument, null);
                    }
                }
            }
        }


    }
}
