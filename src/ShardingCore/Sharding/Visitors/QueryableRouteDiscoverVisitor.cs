using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Exceptions;
using ShardingCore.Extensions;
using ShardingCore.Sharding.Visitors;

namespace ShardingCore.Core.Internal.Visitors
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 28 December 2020 22:09:39
    * @Email: 326308290@qq.com
    */
    public class QueryableRouteShardingTableDiscoverVisitor : ShardingExpressionVisitor
    {
        private static readonly Func<bool, ExpressionType, ShardingOperatorEnum> _shardingOperatorFunc =
            (conditionOnRight, nodeType) =>
            {
                var op = nodeType switch
                {
                    ExpressionType.GreaterThan => conditionOnRight
                        ? ShardingOperatorEnum.GreaterThan
                        : ShardingOperatorEnum.LessThan,
                    ExpressionType.GreaterThanOrEqual => conditionOnRight
                        ? ShardingOperatorEnum.GreaterThanOrEqual
                        : ShardingOperatorEnum.LessThanOrEqual,
                    ExpressionType.LessThan => conditionOnRight
                        ? ShardingOperatorEnum.LessThan
                        : ShardingOperatorEnum.GreaterThan,
                    ExpressionType.LessThanOrEqual => conditionOnRight
                        ? ShardingOperatorEnum.LessThanOrEqual
                        : ShardingOperatorEnum.GreaterThanOrEqual,
                    ExpressionType.Equal => ShardingOperatorEnum.Equal,
                    ExpressionType.NotEqual => ShardingOperatorEnum.NotEqual,
                    _ => ShardingOperatorEnum.UnKnown
                };
                return op;
            };

        private readonly EntityMetadata _entityMetadata;
        private readonly Func<object, ShardingOperatorEnum, string, Func<string, bool>> _keyToTailWithFilter;

        /// <summary>
        /// 是否是分表路由
        /// </summary>
        private readonly bool _shardingTableRoute;

        private LambdaExpression _entityLambdaExpression;
        private readonly ShardingPredicateResult _noShardingPredicateResult = new ShardingPredicateResult(false, null);
        private bool isIgnoreQueryFilter;
        private RoutePredicateExpression _where = RoutePredicateExpression.Default;

        public QueryableRouteShardingTableDiscoverVisitor(EntityMetadata entityMetadata,
            Func<object, ShardingOperatorEnum, string, Func<string, bool>> keyToTailWithFilter, bool shardingTableRoute)
        {
            _entityMetadata = entityMetadata;
            _keyToTailWithFilter = keyToTailWithFilter;
            _shardingTableRoute = shardingTableRoute;
        }

        /// <summary>
        /// 获取路由表达式
        /// </summary>
        /// <returns></returns>
        public RoutePredicateExpression GetRouteParseExpression()
        {
            if (_entityMetadata.QueryFilterExpression != null && !isIgnoreQueryFilter)
            {
                if (_entityLambdaExpression == null)
                {
                    _entityLambdaExpression = _entityMetadata.QueryFilterExpression;
                }
                else
                {
                    var body = Expression.AndAlso(_entityLambdaExpression.Body,
                        _entityMetadata.QueryFilterExpression.Body);
                    _entityLambdaExpression = Expression.Lambda(body, _entityLambdaExpression.Parameters[0]);
                }
            }

            if (_entityLambdaExpression != null)
            {
                var newWhere = Resolve(_entityLambdaExpression);
                _where = _where.And(newWhere);
            }

            return _where;
        }


        private bool IsShardingKey(Expression expression, out ShardingPredicateResult shardingPredicateResult)
        {
            if (expression is MemberExpression member)
            {
                MemberExpression realMember = null;
                if (member.Expression?.Type == _entityMetadata.EntityType)
                {
                    realMember = member;
                }
                else if (MemberExpressionIsConvertAndOriginalIsEntityType(member,out  realMember))
                {
                }

                if (realMember != null)
                {
                    var isShardingKey = false;
                    if (_shardingTableRoute)
                    {
                        isShardingKey = _entityMetadata.ShardingTableProperties.ContainsKey(realMember.Member.Name);
                    }
                    else
                    {
                        isShardingKey = _entityMetadata.ShardingDataSourceProperties.ContainsKey(realMember.Member.Name);
                    }

                    if (isShardingKey)
                    {
                        shardingPredicateResult = new ShardingPredicateResult(true, realMember.Member.Name);
                        return true;
                    }
                }
            }
            // else if (expression is BinaryExpression binaryExpression)
            // {
            //     if (IsShardingKey(binaryExpression.Left, out shardingPredicateResult))
            //     {
            //         return true;
            //     }
            //     else if (IsShardingKey(binaryExpression.Right, out shardingPredicateResult))
            //     {
            //         return true;
            //     }
            // }

            shardingPredicateResult = _noShardingPredicateResult;
            return false;
        }

        /// <summary>
        /// 成员表达式是强转并且强转前的类型是当前对象
        /// </summary>
        /// <param name="member"></param>
        /// <param name="realMember"></param>
        /// <returns></returns>
        private bool MemberExpressionIsConvertAndOriginalIsEntityType(MemberExpression member,
            out MemberExpression realMember)
        {
            if (member.Expression?.NodeType == ExpressionType.Convert &&
                member.Expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.Operand.Type == _entityMetadata.EntityType)
                {
                    realMember = member;
                    return true;
                }

                if (unaryExpression.Operand is MemberExpression m &&
                    m.Expression?.Type == _entityMetadata.EntityType)
                {
                    realMember = m;
                    return true;
                }
            }

            realMember = null;
            return false;
        }

        /// <summary>
        /// 方法是否包含shardingKey xxx.invoke(shardingkey) eg. <code>o=>new[]{}.Contains(o.Id)</code>
        /// </summary>
        /// <param name="methodCallExpression"></param>
        /// <returns></returns>
        private ShardingPredicateResult IsMethodWrapShardingKey(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Arguments.IsNotEmpty())
            {
                for (int i = 0; i < methodCallExpression.Arguments.Count; i++)
                {
                    if (IsShardingKey(methodCallExpression.Arguments[i], out var result))
                        return result;
                }
            }

            return _noShardingPredicateResult;
        }

        private ShardingPredicateResult IsShardingWrapConstant(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Object != null)
            {
                if (IsShardingKey(methodCallExpression.Object, out var result))
                {
                    return result;
                }
            }

            return _noShardingPredicateResult;
        }

        /// <summary>
        /// 表达式是否可以获取值
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool ExpressionCanGetValue(Expression expression)
        { 
            return expression is ConstantExpression
                   || expression is NewExpression
                   || expression is ListInitExpression
                   || expression is NewArrayExpression
                   || (expression is MemberExpression member && (member.Expression is ConstantExpression ||
                                                                 member.Expression is MemberExpression ||
                                                                 member.Expression is MemberExpression))
                   || expression is MethodCallExpression
                   || (expression is UnaryExpression unaryExpression &&
                       unaryExpression.NodeType is ExpressionType.Convert)
                   || expression.NodeType == ExpressionType.ArrayIndex;
        }

        private bool IsMethodCall(Expression expression)
        {
            return expression is MethodCallExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters):
                    isIgnoreQueryFilter = true;
                    break;
                case nameof(Queryable.Where):
                    CombineEntityLambdaExpression(node);
                    break;
            }

            return base.VisitMethodCall(node);
        }

        private void CombineEntityLambdaExpression(MethodCallExpression node)
        {
            if (node.Arguments[1] is UnaryExpression unaryExpression)
            {
                if (unaryExpression.Operand is LambdaExpression lambdaExpression)
                {
                    if (lambdaExpression.Parameters[0].Type == _entityMetadata.EntityType)
                    {
                        if (_entityLambdaExpression == null)
                        {
                            _entityLambdaExpression = lambdaExpression;
                        }
                        else
                        {
                            var body = Expression.AndAlso(_entityLambdaExpression.Body, lambdaExpression.Body);
                            var lambda = Expression.Lambda(body, _entityLambdaExpression.Parameters[0]);
                            _entityLambdaExpression = lambda;
                        }
                    }
                }
            }
        }


        private RoutePredicateExpression Resolve(Expression expression)
        {
            if (expression is LambdaExpression lambda)
            {
                expression = lambda.Body;
                return Resolve(expression);
            }

            //解析左右结构属性判断
            if (expression is BinaryExpression binaryExpression) //解析二元运算符
            {
                return ParsePropertyCondition(binaryExpression);
            }

            if (expression is UnaryExpression unary) //解析一元运算符
            {
                if (unary.Operand is MethodCallExpression unaryCallExpression)
                {
                    // return ResolveLinqToObject(unary.Operand, false);
                    return ResolveInFunc(unaryCallExpression, unary.NodeType != ExpressionType.Not);
                }
            }

            if (expression is MethodCallExpression methodCallExpression) //解析方法
            {
                return ResolveInFunc(methodCallExpression, true);
            }

            return RoutePredicateExpression.Default;
        }

        private RoutePredicateExpression ResolveInFunc(MethodCallExpression methodCallExpression, bool @in)
        {
            if (methodCallExpression.IsEnumerableContains())
            {
                var shardingPredicateResult = IsMethodWrapShardingKey(methodCallExpression);
                if (shardingPredicateResult.IsShardingKey)
                {
                    object arrayObject = null;
                    if (methodCallExpression.Object != null)
                    {
                        if (methodCallExpression.Object is MemberExpression member1Expression)
                        {
                            arrayObject = GetExpressionValue(member1Expression);
                        }
                        else if (methodCallExpression.Object is ListInitExpression member2Expression)
                        {
                            arrayObject = GetExpressionValue(member2Expression);
                        }
                    }
                    else if (methodCallExpression.Arguments[0] is MemberExpression member2Expression)
                    {
                        arrayObject = GetExpressionValue(member2Expression);
                    }
                    else if (methodCallExpression.Arguments[0] is NewArrayExpression member3Expression)
                    {
                        arrayObject = GetExpressionValue(member3Expression);
                    }

                    if (arrayObject != null)
                    {
                        var contains = @in ? RoutePredicateExpression.DefaultFalse : RoutePredicateExpression.Default;


                        if (arrayObject is IEnumerable enumerableObj)
                        {
                            foreach (var shardingValue in enumerableObj)
                            {
                                var eq = _keyToTailWithFilter(shardingValue,
                                    @in ? ShardingOperatorEnum.Equal : ShardingOperatorEnum.NotEqual,
                                    shardingPredicateResult.ShardingPropertyName);
                                if (@in)
                                    contains = contains.Or(new RoutePredicateExpression(eq));
                                else
                                    contains = contains.And(new RoutePredicateExpression(eq));
                            }
                        }

                        return contains;
                    }
                }
            }
            else if (methodCallExpression.IsStringContains())
            {
                if (IsShardingKey(methodCallExpression.Object, out var shardingPredicateResult))
                {
                    if (methodCallExpression.Arguments.Count == 1)
                    {
                        var shardingValue = GetExpressionValue(methodCallExpression.Arguments[0]);
                        var keyToTailWithFilter = _keyToTailWithFilter(shardingValue, ShardingOperatorEnum.AllLike,
                            shardingPredicateResult.ShardingPropertyName);
                        return new RoutePredicateExpression(keyToTailWithFilter);
                    }
                }
            }
            else if (methodCallExpression.IsStringStartWith())
            {
                if (IsShardingKey(methodCallExpression.Object, out var shardingPredicateResult))
                {
                    if (methodCallExpression.Arguments.Count == 1)
                    {
                        var shardingValue = GetExpressionValue(methodCallExpression.Arguments[0]);
                        var keyToTailWithFilter = _keyToTailWithFilter(shardingValue, ShardingOperatorEnum.StartLike,
                            shardingPredicateResult.ShardingPropertyName);
                        return new RoutePredicateExpression(keyToTailWithFilter);
                    }
                }
            }
            else if (methodCallExpression.IsStringEndWith())
            {
                if (IsShardingKey(methodCallExpression.Object, out var shardingPredicateResult))
                {
                    if (methodCallExpression.Arguments.Count == 1)
                    {
                        var shardingValue = GetExpressionValue(methodCallExpression.Arguments[0]);
                        var keyToTailWithFilter = _keyToTailWithFilter(shardingValue, ShardingOperatorEnum.EndLike,
                            shardingPredicateResult.ShardingPropertyName);
                        return new RoutePredicateExpression(keyToTailWithFilter);
                    }
                }
            }
            else if (methodCallExpression.IsNamedEquals())
            {
                //"".equals(o.id)
                var shardingPredicateResult = IsMethodWrapShardingKey(methodCallExpression);
                if (shardingPredicateResult.IsShardingKey)
                {
                    var shardingValue = GetExpressionValue(methodCallExpression.Object);
                    if (shardingValue != null)
                    {
                        var keyToTailWithFilter = _keyToTailWithFilter(shardingValue, ShardingOperatorEnum.Equal,
                            shardingPredicateResult.ShardingPropertyName);
                        return new RoutePredicateExpression(keyToTailWithFilter);
                    }
                }
                else
                {
                    //o.id.equals("")
                    shardingPredicateResult = IsShardingWrapConstant(methodCallExpression);
                    if (shardingPredicateResult.IsShardingKey)
                    {
                        object shardingValue = default;
                        if (methodCallExpression.Arguments[0] is MemberExpression member2Expression)
                        {
                            shardingValue = GetExpressionValue(member2Expression);
                        }
                        else if (methodCallExpression.Arguments[0] is ConstantExpression constantExpression)
                        {
                            shardingValue = GetExpressionValue(constantExpression);
                        }

                        if (shardingValue != default)
                        {
                            var keyToTailWithFilter = _keyToTailWithFilter(shardingValue, ShardingOperatorEnum.Equal,
                                shardingPredicateResult.ShardingPropertyName);
                            return new RoutePredicateExpression(keyToTailWithFilter);
                        }
                    }
                }
            }

            //if (methodCallExpression.IsNamedCompareOrdinal())
            //{

            //}

            //var shardingKeyValue = GetShardingKeyValue(methodCallExpression);
            return RoutePredicateExpression.Default;
        }

        private ShardingOperatorEnum GetParseCompareShardingOperatorEnum(bool conditionOnRight,
            ExpressionType expressionType, int compare)
        {
            if (compare == 1)
            {
                return expressionType switch
                {
                    ExpressionType.GreaterThanOrEqual => conditionOnRight
                        ? ShardingOperatorEnum.GreaterThan
                        : ShardingOperatorEnum.LessThan, //1
                    ExpressionType.GreaterThan => ShardingOperatorEnum.UnKnown, //无
                    ExpressionType.LessThanOrEqual => ShardingOperatorEnum.UnKnown, //1,0,-1 = 无
                    ExpressionType.LessThan => conditionOnRight
                        ? ShardingOperatorEnum.LessThanOrEqual
                        : ShardingOperatorEnum.GreaterThanOrEqual, //0,-1
                    ExpressionType.Equal => conditionOnRight
                        ? ShardingOperatorEnum.GreaterThan
                        : ShardingOperatorEnum.LessThan, //1
                    ExpressionType.NotEqual => ShardingOperatorEnum.NotEqual,
                    _ => ShardingOperatorEnum.UnKnown
                };
            }

            if (compare == 0)
            {
                return expressionType switch
                {
                    ExpressionType.GreaterThanOrEqual => conditionOnRight
                        ? ShardingOperatorEnum.GreaterThanOrEqual
                        : ShardingOperatorEnum.LessThanOrEqual, //0,1
                    ExpressionType.GreaterThan => conditionOnRight
                        ? ShardingOperatorEnum.GreaterThan
                        : ShardingOperatorEnum.LessThan, //1
                    ExpressionType.LessThanOrEqual => conditionOnRight
                        ? ShardingOperatorEnum.LessThanOrEqual
                        : ShardingOperatorEnum.GreaterThanOrEqual, //0,-1
                    ExpressionType.LessThan => conditionOnRight
                        ? ShardingOperatorEnum.LessThan
                        : ShardingOperatorEnum.GreaterThan, //-1
                    ExpressionType.Equal => ShardingOperatorEnum.Equal,
                    ExpressionType.NotEqual => ShardingOperatorEnum.NotEqual,
                    _ => ShardingOperatorEnum.UnKnown
                };
            }

            if (compare == -1)
            {
                return expressionType switch
                {
                    ExpressionType.GreaterThanOrEqual => ShardingOperatorEnum.UnKnown, //-1,0,1
                    ExpressionType.GreaterThan => conditionOnRight
                        ? ShardingOperatorEnum.GreaterThanOrEqual
                        : ShardingOperatorEnum.LessThanOrEqual, //0,1
                    ExpressionType.LessThanOrEqual => conditionOnRight
                        ? ShardingOperatorEnum.LessThan
                        : ShardingOperatorEnum.GreaterThan, //-1
                    ExpressionType.LessThan => ShardingOperatorEnum.UnKnown, //无
                    ExpressionType.Equal => conditionOnRight
                        ? ShardingOperatorEnum.LessThan
                        : ShardingOperatorEnum.GreaterThan, //1
                    ExpressionType.NotEqual => ShardingOperatorEnum.NotEqual,
                    _ => ShardingOperatorEnum.UnKnown
                };
            }

            return ShardingOperatorEnum.UnKnown;
        }

        private RoutePredicateExpression ParseCompare(MethodCallExpression methodCallExpression, Expression left,
            Expression right, ExpressionType expressionType, int compare)
        {
            if (left.Type == right.Type)
            {
                if (methodCallExpression.Method.ReturnType == typeof(int))
                {
                    return ParseCondition0(left, right, expressionType,
                        (conditionOnRight, nodeType) =>
                            GetParseCompareShardingOperatorEnum(conditionOnRight, nodeType, compare));
                }
            }

            return RoutePredicateExpression.Default;
        }

        private RoutePredicateExpression ParseConditionOnRight0(bool conditionOnRight,
            ShardingPredicateResult predicateLeftResult,
            Expression conditionExpression, ExpressionType expressionType)
        {
            if (ExpressionCanGetValue(conditionExpression))
            {
                var shardingPropertyName = predicateLeftResult.ShardingPropertyName;
                var value = GetExpressionValue(conditionExpression);

                if (shardingPropertyName == null || value == default)
                    return RoutePredicateExpression.Default;
                var op = _shardingOperatorFunc(conditionOnRight, expressionType);


                return new RoutePredicateExpression(_keyToTailWithFilter(value, op, shardingPropertyName));
            }
            else
                return RoutePredicateExpression.Default;
        }

        private RoutePredicateExpression ParseCondition0(Expression left, Expression right,
            ExpressionType expressionType, Func<bool, ExpressionType, ShardingOperatorEnum> shardingOperatorFunc)
        {
            if (IsShardingKey(left, out var predicateLeftResult))
            {
                return ParseConditionOnRight0(true, predicateLeftResult, right, expressionType);
            }
            else if (IsShardingKey(right, out var predicateRightResult))
            {
                return ParseConditionOnRight0(false, predicateRightResult, left, expressionType);
            }
            else
                return RoutePredicateExpression.Default;
        }

        private RoutePredicateExpression ParseNamedComparison(BinaryExpression binaryExpression,
            MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.GetComparisonLeftAndRight(out var result))
            {
                return ParseCompare(methodCallExpression, result.Left, result.Right,
                    binaryExpression.NodeType, (int)GetExpressionValue(binaryExpression.Right));
            }

            return RoutePredicateExpression.Default;
        }


        private RoutePredicateExpression ParsePropertyCondition(BinaryExpression binaryExpression)
        {
            // RoutePredicateExpression left = RoutePredicateExpression.Default;
            // RoutePredicateExpression right = RoutePredicateExpression.Default;
            //左边是属性判断是否是分片的
            if (IsShardingKey(binaryExpression.Left, out var predicateLeftResult))
            {
                return ParseConditionOnRight0(true, predicateLeftResult, binaryExpression.Right,
                    binaryExpression.NodeType);
            }
            else if (IsShardingKey(binaryExpression.Right, out var predicateRightResult))
            {
                return ParseConditionOnRight0(false, predicateRightResult, binaryExpression.Left,
                    binaryExpression.NodeType);
            }
            else if (binaryExpression.IsNamedComparison(out var methodCallExpression))
            {
                return ParseNamedComparison(binaryExpression, methodCallExpression);
            }
            else
            {
                RoutePredicateExpression left = RoutePredicateExpression.Default;
                RoutePredicateExpression right = RoutePredicateExpression.Default;

                //递归获取
                if (binaryExpression.Left is BinaryExpression binaryExpression1)
                    left = ParsePropertyCondition(binaryExpression1);
                if (binaryExpression.Right is BinaryExpression binaryExpression2)
                    right = ParsePropertyCondition(binaryExpression2);

                if (binaryExpression.Left is MethodCallExpression methodCallLeftExpression)
                {
                    if (!methodCallLeftExpression.IsNamedComparison())
                    {
                        left = Resolve(methodCallLeftExpression);
                    }
                }

                if (binaryExpression.Right is MethodCallExpression methodCallRightExpression)
                {
                    if (!methodCallRightExpression.IsNamedComparison())
                    {
                        right = Resolve(methodCallRightExpression);
                    }
                }

                if (binaryExpression.Left is UnaryExpression unaryExpression1 &&
                    (binaryExpression.Right is MemberExpression))
                    left = Resolve(unaryExpression1);
                if (binaryExpression.Right is UnaryExpression unaryExpression2 &&
                    (binaryExpression.Left is MemberExpression))
                    right = Resolve(unaryExpression2);

                //组合
                if (binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    return left.And(right);
                }
                else if (binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    return left.Or(right);
                }
                else
                {
                    return RoutePredicateExpression.Default;
                }
            }
        }
    }


    /// <summary>
    /// 分片条件结果
    /// </summary>
    internal class ShardingPredicateResult
    {
        public ShardingPredicateResult(bool isShardingKey, string shardingPropertyName)
        {
            IsShardingKey = isShardingKey;
            ShardingPropertyName = shardingPropertyName;
        }

        /// <summary>
        /// 是否是分片字段
        /// </summary>
        public bool IsShardingKey { get; }

        /// <summary>
        /// 分片字段名称
        /// </summary>
        public string ShardingPropertyName { get; }
    }
}