using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.Visitors
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 13 January 2021 16:26:41
* @Email: 326308290@qq.com
*/
#if !EFCORE5
    /// <summary>
    /// 获取分表类型
    /// </summary>
    internal class ShardingEntitiesVisitor : ExpressionVisitor
    {
        private readonly ISet<Type> _shardingEntities = new HashSet<Type>();


        public ISet<Type> GetShardingEntities()
        {
            return _shardingEntities;
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable&&queryable.ElementType.IsShardingEntity())
            {
                _shardingEntities.Add(queryable.ElementType);
            }

            return base.VisitConstant(node);
        }
    }
#endif
        
        #if EFCORE5
    /// <summary>
    /// 获取分表类型
    /// </summary>
    internal class ShardingEntitiesVisitor : ExpressionVisitor
    {
        private readonly ISet<Type> _shardingEntities = new HashSet<Type>();


        public ISet<Type> GetShardingEntities()
        {
            return _shardingEntities;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is QueryRootExpression queryRootExpression&&queryRootExpression.EntityType.ClrType.IsShardingEntity())
            {
                _shardingEntities.Add(queryRootExpression.EntityType.ClrType);
            }
            return base.VisitExtension(node);
        }
    }
    #endif
        // internal class ShardingEntitiesVisitor : ExpressionVisitor
        // {
        //     private readonly IVirtualTableManager _virtualTableManager;
        //     private readonly ISet<Type> _shardingEntities = new HashSet<Type>();
        //
        //     public ShardingEntitiesVisitor(IVirtualTableManager virtualTableManager)
        //     {
        //         _virtualTableManager = virtualTableManager;
        //     }
        //
        //     public ISet<Type> GetShardingEntities()
        //     {
        //         return _shardingEntities;
        //     }
        //
        //     private bool IsShardingKey(Expression expression, out Type shardingEntity)
        //     {
        //         if (expression is MemberExpression member
        //             && _virtualTableManager.IsShardingKey(member.Expression.Type, member.Member.Name))
        //         {
        //             shardingEntity = member.Expression.Type;
        //             return true;
        //         }
        //
        //         shardingEntity = null;
        //         return false;
        //     }
        //
        //     private bool IsMethodShardingKey(MethodCallExpression methodCallExpression, out Type shardingEntity)
        //     {
        //         if (methodCallExpression.Arguments.IsNotEmpty())
        //         {
        //             for (int i = 0; i < methodCallExpression.Arguments.Count; i++)
        //             {
        //                 if (methodCallExpression.Arguments[i] is MemberExpression member
        //                     && _virtualTableManager.IsShardingKey(member.Expression.Type, member.Member.Name))
        //                 {
        //                     shardingEntity = member.Expression.Type;
        //                     return true;
        //                 }
        //             }
        //         }
        //
        //         shardingEntity = null;
        //         return false;
        //     }
        //
        //     protected override Expression VisitMethodCall(MethodCallExpression node)
        //     {
        //         var methodName = node.Method.Name;
        //         switch (methodName)
        //         {
        //             case nameof(Queryable.Where):
        //                 ResolveWhere(node);
        //                 break;
        //         }
        //
        //
        //         return base.VisitMethodCall(node);
        //     }
        //
        //     private void ResolveWhere(MethodCallExpression node)
        //     {
        //         if (node.Arguments[1] is UnaryExpression unaryExpression)
        //         {
        //             if (unaryExpression.Operand is LambdaExpression lambdaExpression)
        //             {
        //                 Resolve(lambdaExpression);
        //             }
        //         }
        //     }
        //
        //
        //     private void Resolve(Expression expression)
        //     {
        //         if (expression is LambdaExpression)
        //         {
        //             LambdaExpression lambda = expression as LambdaExpression;
        //             expression = lambda.Body;
        //             Resolve(expression);
        //         }
        //
        //         if (expression is BinaryExpression binaryExpression) //解析二元运算符
        //         {
        //             ParseGetWhere(binaryExpression);
        //         }
        //
        //         if (expression is UnaryExpression) //解析一元运算符
        //         {
        //             UnaryExpression unary = expression as UnaryExpression;
        //             if (unary.Operand is MethodCallExpression methodCall1Expression)
        //             {
        //                 ResolveInFunc(methodCall1Expression, unary.NodeType != ExpressionType.Not);
        //             }
        //         }
        //
        //         if (expression is MethodCallExpression methodCallExpression) //解析扩展方法
        //         {
        //             ResolveInFunc(methodCallExpression, true);
        //         }
        //     }
        //
        //     private void ResolveInFunc(MethodCallExpression methodCallExpression, bool @in)
        //     {
        //         if (methodCallExpression.IsEnumerableContains(methodCallExpression.Method.Name) && IsMethodShardingKey(methodCallExpression, out var shardingEntity))
        //         {
        //             _shardingEntities.Add(shardingEntity);
        //         }
        //     }
        //
        //     private void ParseGetWhere(BinaryExpression binaryExpression)
        //     {
        //         //递归获取
        //         if (binaryExpression.Left is BinaryExpression)
        //             ParseGetWhere(binaryExpression.Left as BinaryExpression);
        //         if (binaryExpression.Left is MethodCallExpression methodCallExpression)
        //         {
        //             Resolve(methodCallExpression);
        //         }
        //
        //         if (binaryExpression.Left is UnaryExpression unaryExpression)
        //             Resolve(unaryExpression);
        //
        //         if (binaryExpression.Right is BinaryExpression)
        //             ParseGetWhere(binaryExpression.Right as BinaryExpression);
        //
        //         if (IsShardingKey(binaryExpression.Left, out var shardingEntity1))
        //         {
        //             _shardingEntities.Add(shardingEntity1);
        //         }
        //         else if (IsShardingKey(binaryExpression.Right, out var shardingEntity2))
        //         {
        //             _shardingEntities.Add(shardingEntity2);
        //         }
        //     }
        // }
}