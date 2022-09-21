using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core.TrackerManagers;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.Visitors.Querys
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Saturday, 20 February 2021 11:14:35
    * @Email: 326308290@qq.com
    */
#if !EFCORE2 && !EFCORE3 && !EFCORE5 && !EFCORE6
    error
#endif
#if EFCORE2 || EFCORE3
    /// <summary>
    /// 获取分表类型
    /// </summary>
    internal class QueryEntitiesVisitor : ExpressionVisitor
    {
        private readonly ITrackerManager _trackerManager;
        private readonly ISet<Type> _shardingEntities = new HashSet<Type>();

        public QueryEntitiesVisitor(ITrackerManager trackerManager)
        {
            _trackerManager = trackerManager;
        }


        public ISet<Type> GetQueryEntities()
        {
            return _shardingEntities;
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable)
            {
                _shardingEntities.Add(queryable.ElementType);
            }

            return base.VisitConstant(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var methodName = node.Method.Name;
            if (methodName == nameof(EntityFrameworkQueryableExtensions.Include) || methodName == nameof(EntityFrameworkQueryableExtensions.ThenInclude))
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
                                _shardingEntities.Add(argument);
                            }
                        }
                    }

                    if (!genericArgument.IsSimpleType())
                    {
                        //if is db context model
                        if (_trackerManager.IsDbContextModel(genericArgument))
                        {
                            _shardingEntities.Add(genericArgument);
                        }
                    }
                }
            }
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitMember
            (MemberExpression memberExpression)
        {
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
                        _shardingEntities.Add(queryable.ElementType);
                    }
                    //return Expression.Constant(value);
                }
                if (member is PropertyInfo propertyInfo)
                {
                    object value = propertyInfo.GetValue(container, null);
                    if (value is IQueryable queryable)
                    {
                        _shardingEntities.Add(queryable.ElementType);
                    }
                }
            }
            return base.VisitMember(memberExpression);
        }
    }
#endif

#if EFCORE5 || EFCORE6
    /// <summary>
    /// 获取分表类型
    /// </summary>
    internal class QueryEntitiesVisitor : ExpressionVisitor
    {
        private readonly ITrackerManager _trackerManager;
        private readonly ISet<Type> _shardingEntities = new HashSet<Type>();

        public QueryEntitiesVisitor(ITrackerManager trackerManager)
        {
            _trackerManager = trackerManager;
        }

        public ISet<Type> GetQueryEntities()
        {
            return _shardingEntities;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is QueryRootExpression queryRootExpression)
            {
                _shardingEntities.Add(queryRootExpression.EntityType.ClrType);
            }
            return base.VisitExtension(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var methodName = node.Method.Name;
            if (methodName == nameof(EntityFrameworkQueryableExtensions.Include) || methodName == nameof(EntityFrameworkQueryableExtensions.ThenInclude))
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
                                _shardingEntities.Add(argument);
                            }
                        }
                    }

                    if (!genericArgument.IsSimpleType())
                    {
                        //if is db context model
                        if (_trackerManager.IsDbContextModel(genericArgument))
                        {
                            _shardingEntities.Add(genericArgument);
                        }
                    }
                }
            }
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitMember
            (MemberExpression memberExpression)
        {
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
                        _shardingEntities.Add(queryable.ElementType);
                    }
                    //return Expression.Constant(value);
                }
                if (member is PropertyInfo propertyInfo)
                {
                    object value = propertyInfo.GetValue(container, null);
                    if (value is IQueryable queryable)
                    {
                        _shardingEntities.Add(queryable.ElementType);
                    }
                }
            }
            return base.VisitMember(memberExpression);
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