//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using Microsoft.EntityFrameworkCore.Query;
//using ShardingCore.Extensions;

//namespace ShardingCore.Core.Internal.Visitors
//{
///*
//* @Author: xjm
//* @Description:
//* @Date: Wednesday, 13 January 2021 16:26:41
//* @Email: 326308290@qq.com
//*/
//#if !EFCORE5
//    /// <summary>
//    /// 获取分表类型
//    /// </summary>
//    internal class ShardingEntitiesVisitor : ExpressionVisitor
//    {
//        private readonly ISet<Type> _shardingEntities = new HashSet<Type>();


//        public ISet<Type> GetShardingEntities()
//        {
//            return _shardingEntities;
//        }
//        protected override Expression VisitConstant(ConstantExpression node)
//        {
//            if (node.Value is IQueryable queryable&&queryable.ElementType.IsShardingTable())
//            {
//                _shardingEntities.Add(queryable.ElementType);
//            }

//            return base.VisitConstant(node);
//        }
//    }
//#endif
        
//        #if EFCORE5
//    /// <summary>
//    /// 获取分表类型
//    /// </summary>
//    internal class ShardingEntitiesVisitor : ExpressionVisitor
//    {
//        private readonly ISet<Type> _shardingEntities = new HashSet<Type>();


//        public ISet<Type> GetShardingEntities()
//        {
//            return _shardingEntities;
//        }

//        protected override Expression VisitExtension(Expression node)
//        {
//            if (node is QueryRootExpression queryRootExpression&&queryRootExpression.EntityType.ClrType.IsShardingTable())
//            {
//                _shardingEntities.Add(queryRootExpression.EntityType.ClrType);
//            }
//            return base.VisitExtension(node);
//        }
//    }
//    #endif
//}