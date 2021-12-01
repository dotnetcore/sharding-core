using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;

namespace ShardingCore.Core.Internal.Visitors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 28 December 2020 22:09:39
    * @Email: 326308290@qq.com
    */
    public class QueryableRouteShardingTableDiscoverVisitor<TKey> : ExpressionVisitor
    {

        private static readonly ConcurrentDictionary<Expression<Func<string, bool>>, Func<string, bool>> _routeFilter =
            new ConcurrentDictionary<Expression<Func<string, bool>>, Func<string, bool>>(new RouteFilterComparer());
        private readonly EntityMetadata _entityMetadata;
        private readonly Func<TKey, ShardingOperatorEnum, Expression<Func<string, bool>>> _keyToTailWithFilter;
        /// <summary>
        /// 是否是分表路由
        /// </summary>
        private readonly bool _shardingTableRoute;
        private Expression<Func<string, bool>> _where = x => true;

        public QueryableRouteShardingTableDiscoverVisitor(EntityMetadata entityMetadata, Func<TKey, ShardingOperatorEnum, Expression<Func<string, bool>>> keyToTailWithFilter, bool shardingTableRoute)
        {
            _entityMetadata = entityMetadata;
            _keyToTailWithFilter = keyToTailWithFilter;
            _shardingTableRoute = shardingTableRoute;
        }

        public Func<string, bool> GetStringFilterTail()
        {
            //if (_lastExpression == null)
            //{
            //    _lastExpression = _where;
            //}
            //else
            //{
            //    var startNew = Stopwatch.StartNew();
            //    for (int i = 0; i < 300; i++)
            //    {
            //        var exxq = LambdaCompare.Eq(_lastExpression, _where);
            //    }
            //    startNew.Stop();
            //    var x = startNew.ElapsedMilliseconds;
            //    var eq = LambdaCompare.Eq(_lastExpression, _where);
            //    _lastExpression = _where;
            //}
            //return _routeFilter.GetOrAdd(_where, key => _where.Compile());
            return _where.Compile();
        }

        private bool IsShardingKey(Expression expression)
        {
            return expression is MemberExpression member
                   && member.Expression.Type == _entityMetadata.EntityType
                   && member.Member.Name == (_shardingTableRoute ? _entityMetadata.ShardingTableProperty.Name : _entityMetadata.ShardingDataSourceProperty.Name);
        }
        /// <summary>
        /// 方法是否包含shardingKey
        /// </summary>
        /// <param name="methodCallExpression"></param>
        /// <returns></returns>
        private bool IsMethodWrapShardingKey(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Arguments.IsNotEmpty())
            {
                for (int i = 0; i < methodCallExpression.Arguments.Count; i++)
                {
                    var isShardingKey = methodCallExpression.Arguments[i] is MemberExpression member
                                        && member.Expression.Type == _entityMetadata.EntityType
                                        && member.Member.Name == (_shardingTableRoute ? _entityMetadata.ShardingTableProperty.Name : _entityMetadata.ShardingDataSourceProperty.Name);
                    if (isShardingKey) return true;
                }
            }

            return false;
        }
        private bool IsConstantOrMember(Expression expression)
        {
            return expression is ConstantExpression
                   || (expression is MemberExpression member && (member.Expression is ConstantExpression || member.Expression is MemberExpression || member.Expression is MemberExpression))
                   || expression is MethodCallExpression;
        }

        private bool IsMethodCall(Expression expression)
        {
            return expression is MethodCallExpression;
        }

        private TKey GetShardingKeyValue(Expression expression)
        {
            if (expression is ConstantExpression constantExpression)
                return (TKey)constantExpression.Value;
            if (expression is UnaryExpression unaryExpression)
            {
                return Expression.Lambda<Func<TKey>>(unaryExpression.Operand).Compile()();
            }

            if (expression is MemberExpression member1Expression)
            {
                //var target = GetShardingKeyValue(member1Expression.Expression);
                //if (member1Expression.Member is FieldInfo field)
                //    return field.GetValue(target);
                //if (member1Expression.Member is PropertyInfo property)
                //    return property.GetValue(target);
                return Expression.Lambda<Func<TKey>>(member1Expression).Compile()();
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                return Expression.Lambda<Func<TKey>>(methodCallExpression).Compile()();
                //return methodCallExpression.Method.Invoke(
                //    GetShardingKeyValue(methodCallExpression.Object),
                //    methodCallExpression.Arguments
                //        .Select(
                //            a => GetShardingKeyValue(a)
                //        )
                //        .ToArray()
                //);
            }

            throw new ShardingCoreException("cant get value " + expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Queryable.Where))
            {
                if (node.Arguments[1] is UnaryExpression unaryExpression)
                {
                    if (unaryExpression.Operand is LambdaExpression lambdaExpression)
                    {
                        var newWhere = Resolve(lambdaExpression);
                        _where = _where.And(newWhere);
                    }
                }
            }

            return base.VisitMethodCall(node);
        }


        private Expression<Func<string, bool>> Resolve(Expression expression)
        {
            if (expression is LambdaExpression lambda)
            {
                expression = lambda.Body;
                return Resolve(expression);
            }

            if (expression is BinaryExpression binaryExpression) //解析二元运算符
            {
                return ParseGetWhere(binaryExpression);
            }

            if (expression is UnaryExpression unary) //解析一元运算符
            {
                if (unary.Operand is MethodCallExpression unaryCallExpression)
                {
                    // return ResolveLinqToObject(unary.Operand, false);
                    return ResolveInFunc(unaryCallExpression, unary.NodeType != ExpressionType.Not);
                }
            }

            if (expression is MethodCallExpression methodCallExpression) //解析扩展方法
            {
                return ResolveInFunc(methodCallExpression, true);
            }
            return o => true;
        }

        private Expression<Func<string, bool>> ResolveInFunc(MethodCallExpression methodCallExpression, bool @in)
        {
            if (methodCallExpression.IsEnumerableContains(methodCallExpression.Method.Name) && IsMethodWrapShardingKey(methodCallExpression))
            {
                object arrayObject = null;
                if (methodCallExpression.Object != null)
                {
                    if (methodCallExpression.Object is MemberExpression member1Expression)
                    {
                        arrayObject = Expression.Lambda(member1Expression).Compile().DynamicInvoke();
                    }
                    else if (methodCallExpression.Object is ListInitExpression member2Expression)
                    {
                        arrayObject = Expression.Lambda(member2Expression).Compile().DynamicInvoke();
                    }
                }
                else if (methodCallExpression.Arguments[0] is MemberExpression member2Expression)
                {
                    arrayObject = Expression.Lambda(member2Expression).Compile().DynamicInvoke();
                }
                else if (methodCallExpression.Arguments[0] is NewArrayExpression member3Expression)
                {
                    arrayObject = Expression.Lambda(member3Expression).Compile().DynamicInvoke();
                }

                if (arrayObject != null)
                {
                    var enumerable = (IEnumerable<TKey>)arrayObject;
                    Expression<Func<string, bool>> contains = x => false;
                    if (!@in)
                        contains = x => true;
                    foreach (var item in enumerable)
                    {
                        var eq = _keyToTailWithFilter(item, @in ? ShardingOperatorEnum.Equal : ShardingOperatorEnum.NotEqual);
                        if (@in)
                            contains = contains.Or(eq);
                        else
                            contains = contains.And(eq);
                    }

                    return contains;
                }
            }

            return x => true;
        }

        private Expression<Func<string, bool>> ParseGetWhere(BinaryExpression binaryExpression)
        {
            Expression<Func<string, bool>> left = x => true;
            Expression<Func<string, bool>> right = x => true;

            //递归获取
            if (binaryExpression.Left is BinaryExpression)
                left = ParseGetWhere(binaryExpression.Left as BinaryExpression);
            if (binaryExpression.Left is MethodCallExpression methodCallExpression)
                left = Resolve(methodCallExpression);

            if (binaryExpression.Left is UnaryExpression unaryExpression)
                left = Resolve(unaryExpression);

            if (binaryExpression.Right is BinaryExpression)
                right = ParseGetWhere(binaryExpression.Right as BinaryExpression);

            //组合
            if (binaryExpression.NodeType == ExpressionType.AndAlso)
            {
                return left.And(right);
            }
            else if (binaryExpression.NodeType == ExpressionType.OrElse)
            {
                return left.Or(right);
            }
            //单个
            else
            {
                //条件在右边
                bool conditionOnRight = false;
                TKey value = default;

                if (IsShardingKey(binaryExpression.Left) && IsConstantOrMember(binaryExpression.Right))
                {
                    conditionOnRight = true;
             
                    value = GetShardingKeyValue(binaryExpression.Right);
                }
                else if (IsConstantOrMember(binaryExpression.Left) && IsShardingKey(binaryExpression.Right))
                {
                    conditionOnRight = false;
                    value = GetShardingKeyValue(binaryExpression.Left);
                }
                else
                    return x => true;

                var op = binaryExpression.NodeType switch
                {
                    ExpressionType.GreaterThan => conditionOnRight ? ShardingOperatorEnum.GreaterThan : ShardingOperatorEnum.LessThan,
                    ExpressionType.GreaterThanOrEqual => conditionOnRight ? ShardingOperatorEnum.GreaterThanOrEqual : ShardingOperatorEnum.LessThanOrEqual,
                    ExpressionType.LessThan => conditionOnRight ? ShardingOperatorEnum.LessThan : ShardingOperatorEnum.GreaterThan,
                    ExpressionType.LessThanOrEqual => conditionOnRight ? ShardingOperatorEnum.LessThanOrEqual : ShardingOperatorEnum.GreaterThanOrEqual,
                    ExpressionType.Equal => ShardingOperatorEnum.Equal,
                    ExpressionType.NotEqual => ShardingOperatorEnum.NotEqual,
                    _ => ShardingOperatorEnum.UnKnown
                };

                if (EqualityComparer<TKey>.Default.Equals(value, default))
                    return x => true;


                return _keyToTailWithFilter(value, op);
            }
        }
    }
}