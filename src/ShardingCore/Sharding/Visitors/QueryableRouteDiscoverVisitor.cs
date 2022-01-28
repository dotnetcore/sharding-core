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
using ShardingCore.Core.VirtualRoutes.TableRoutes;
using ShardingCore.Core.VirtualTables;
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
    public class QueryableRouteShardingTableDiscoverVisitor : ExpressionVisitor
    {

        private readonly EntityMetadata _entityMetadata;
        private readonly Func<object, ShardingOperatorEnum, string, Expression<Func<string, bool>>> _keyToTailWithFilter;
        /// <summary>
        /// 是否是分表路由
        /// </summary>
        private readonly bool _shardingTableRoute;
        private Expression<Func<string, bool>> _where = x => true;
        private LambdaExpression _entityLambdaExpression;
        private readonly ShardingPredicateResult _noShardingPredicateResult = new ShardingPredicateResult(false, null);
        private bool isIgnoreQueryFilter;

        public QueryableRouteShardingTableDiscoverVisitor(EntityMetadata entityMetadata, Func<object, ShardingOperatorEnum, string, Expression<Func<string, bool>>> keyToTailWithFilter, bool shardingTableRoute)
        {
            _entityMetadata = entityMetadata;
            _keyToTailWithFilter = keyToTailWithFilter;
            _shardingTableRoute = shardingTableRoute;
        }

        public Expression<Func<string, bool>> GetRouteParseExpression()
        {

            if (_entityMetadata.QueryFilterExpression != null&&!isIgnoreQueryFilter)
            {
                if (_entityLambdaExpression == null)
                {
                    _entityLambdaExpression = _entityMetadata.QueryFilterExpression;
                }
                else
                {
                    var body = Expression.AndAlso(_entityLambdaExpression.Body, _entityMetadata.QueryFilterExpression.Body);
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


        private ShardingPredicateResult IsShardingKey(Expression expression)
        {
            if (expression is MemberExpression member)
            {
                if (member.Expression.Type == _entityMetadata.EntityType)
                {
                    var isShardingKey = false;
                    if (_shardingTableRoute)
                    {
                        isShardingKey = _entityMetadata.ShardingTableProperties.ContainsKey(member.Member.Name);
                    }
                    else
                    {
                        isShardingKey = _entityMetadata.ShardingDataSourceProperties.ContainsKey(member.Member.Name);
                    }
                    return new ShardingPredicateResult(isShardingKey, isShardingKey ? member.Member.Name : null);
                }
            }

            return _noShardingPredicateResult;
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
                    var result = IsShardingKey(methodCallExpression.Arguments[i]);
                    if (result.IsShardingKey)
                        return result;
                }
            }
            return _noShardingPredicateResult;
        }

        private ShardingPredicateResult IsShardingWrapConstant(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Object != null)
            {
                return IsShardingKey(methodCallExpression.Object);
            }
            return _noShardingPredicateResult;
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

        private object GetShardingKeyValue(Expression expression)
        {
            if (expression == null)
                return null;
            switch (expression)
            {
                case ConstantExpression e:
                    return e.Value;

                case MemberExpression e when e.Member is FieldInfo field:
                    return field.GetValue(
                        GetShardingKeyValue(
                            e.Expression
                        )
                    );

                case MemberExpression e when e.Member is PropertyInfo property:
                    return property.GetValue(
                        GetShardingKeyValue(
                            e.Expression
                        )
                    );

                case ListInitExpression e when e.NewExpression.Arguments.Count() == 0:
                    {
                        var collection = e.NewExpression.Constructor.Invoke(new object[0]);
                        foreach (var i in e.Initializers)
                        {
                            i.AddMethod.Invoke(
                                collection,
                                i.Arguments
                                    .Select(
                                        a => GetShardingKeyValue(a)
                                    )
                                    .ToArray()
                            );
                        }
                        return collection;
                    }
                case NewArrayExpression e when e.NodeType == ExpressionType.NewArrayInit && e.Expressions.Count > 0:
                    {
                        var collection = new List<object>(e.Expressions.Count);
                        foreach (var arrayItemExpression in e.Expressions)
                        {
                            collection.Add(GetShardingKeyValue(arrayItemExpression));
                        }
                        return collection;
                    }


                case MethodCallExpression e:
                    return e.Method.Invoke(
                        GetShardingKeyValue(e.Object),
                        e.Arguments
                            .Select(
                                a => GetShardingKeyValue(a)
                            )
                            .ToArray()
                    );

                default:
                    //TODO: better messaging
                    throw new ShardingCoreException("cant get value " + expression);
            }
            //if (expression is ConstantExpression constantExpression)
            //{
            //    return constantExpression.Value;
            //}
            //if (expression is UnaryExpression unaryExpression)
            //{
            //    return Expression.Lambda(unaryExpression.Operand).Compile().DynamicInvoke();
            //}

            //if (expression is MemberExpression member1Expression)
            //{

            //    if (member1Expression.Expression is ConstantExpression memberConstantExpression)
            //    {
            //        if (member1Expression.Member is FieldInfo memberFieldInfo)
            //        {
            //            object container = memberConstantExpression.Value;
            //            return memberFieldInfo.GetValue(container);
            //        }
            //        if (member1Expression.Member is PropertyInfo memberPropertyInfo)
            //        {
            //            object container = memberConstantExpression.Value;
            //            return memberPropertyInfo.GetValue(container);
            //        }
            //        else
            //        {
            //            return memberConstantExpression.Value;
            //        }
            //    }
            //    return Expression.Lambda(member1Expression).Compile().DynamicInvoke();
            //}

            //if (expression is MethodCallExpression methodCallExpression)
            //{
            //    return Expression.Lambda(methodCallExpression).Compile().DynamicInvoke();
            //    //return methodCallExpression.Method.Invoke(
            //    //    GetShardingKeyValue(methodCallExpression.Object),
            //    //    methodCallExpression.Arguments
            //    //        .Select(
            //    //            a => GetShardingKeyValue(a)
            //    //        )
            //    //        .ToArray()
            //    //);
            //}

            //throw new ShardingCoreException("cant get value " + expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters): isIgnoreQueryFilter = true;break;
                case nameof(Queryable.Where): CombineEntityLambdaExpression(node);break;
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
                    //var newWhere = DoResolve(lambdaExpression);
                    //_where = _where.And(newWhere);
                }
            }
        }

        //private Expression<Func<string, bool>> DoResolve(LambdaExpression lambdaExpression)
        //{

        //    if (!useQueryFilterOnFirstWhere)
        //    {
        //        useQueryFilterOnFirstWhere = true;
        //        if (_entityMetadata.QueryFilterExpression != null)
        //        {
        //            var body = Expression.AndAlso(lambdaExpression.Body, _entityMetadata.QueryFilterExpression.Body);
        //            var lambda = Expression.Lambda(body, lambdaExpression.Parameters[0]);
        //            return Resolve(lambda);
        //        }
        //    }
        //    return Resolve(lambdaExpression);
        //}


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
            if (methodCallExpression.IsEnumerableContains(methodCallExpression.Method.Name))
            {
                var shardingPredicateResult = IsMethodWrapShardingKey(methodCallExpression);
                if (shardingPredicateResult.IsShardingKey)
                {
                    object arrayObject = null;
                    if (methodCallExpression.Object != null)
                    {
                        if (methodCallExpression.Object is MemberExpression member1Expression)
                        {
                            arrayObject = GetShardingKeyValue(member1Expression);
                        }
                        else if (methodCallExpression.Object is ListInitExpression member2Expression)
                        {
                            arrayObject = GetShardingKeyValue(member2Expression);
                        }
                    }
                    else if (methodCallExpression.Arguments[0] is MemberExpression member2Expression)
                    {
                        arrayObject = GetShardingKeyValue(member2Expression);
                    }
                    else if (methodCallExpression.Arguments[0] is NewArrayExpression member3Expression)
                    {
                        arrayObject = GetShardingKeyValue(member3Expression);
                    }

                    if (arrayObject != null)
                    {
                        Expression<Func<string, bool>> contains = x => false;
                        if (!@in)
                            contains = x => true;
                        //if (arrayObject is IEnumerable<TKey> enumerableKey)
                        //{
                        //    foreach (var item in enumerableKey)
                        //    {
                        //        var eq = _keyToTailWithFilter(item, @in ? ShardingOperatorEnum.Equal : ShardingOperatorEnum.NotEqual);
                        //        if (@in)
                        //            contains = contains.Or(eq);
                        //        else
                        //            contains = contains.And(eq);
                        //    }

                        //}else if (arrayObject is IEnumerable enumerableObj)
                        //{
                        //    foreach (var item in enumerableObj)
                        //    {
                        //        var eq = _keyToTailWithFilter((TKey)item, @in ? ShardingOperatorEnum.Equal : ShardingOperatorEnum.NotEqual);
                        //        if (@in)
                        //            contains = contains.Or(eq);
                        //        else
                        //            contains = contains.And(eq);
                        //    }
                        //}

                        if (arrayObject is IEnumerable enumerableObj)
                        {
                            foreach (var shardingValue in enumerableObj)
                            {
                                var eq = _keyToTailWithFilter(shardingValue, @in ? ShardingOperatorEnum.Equal : ShardingOperatorEnum.NotEqual, shardingPredicateResult.ShardingPropertyName);
                                if (@in)
                                    contains = contains.Or(eq);
                                else
                                    contains = contains.And(eq);
                            }
                        }
                        return contains;
                    }
                }
            }

            if (methodCallExpression.IsNamedEquals())
            {
                //"".equals(o.id)
                var shardingPredicateResult = IsMethodWrapShardingKey(methodCallExpression);
                if (shardingPredicateResult.IsShardingKey)
                {
                    if (methodCallExpression.Object is ConstantExpression constantExpression)
                    {
                        var shardingValue = constantExpression.Value;
                        return _keyToTailWithFilter(shardingValue, ShardingOperatorEnum.Equal, shardingPredicateResult.ShardingPropertyName);
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
                            shardingValue = GetShardingKeyValue(member2Expression);
                        }
                        else if (methodCallExpression.Arguments[0] is ConstantExpression constantExpression)
                        {
                            shardingValue = GetShardingKeyValue(constantExpression);
                        }

                        if (shardingValue != default)
                        {
                            return _keyToTailWithFilter(shardingValue, ShardingOperatorEnum.Equal, shardingPredicateResult.ShardingPropertyName);
                        }
                    }
                }
            }

            //var shardingKeyValue = GetShardingKeyValue(methodCallExpression);
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
            if (binaryExpression.Right is BinaryExpression binaryExpression2)
                right = ParseGetWhere(binaryExpression2);

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
                string shardingPropertyName = null;
                object value = default;

                if (IsConstantOrMember(binaryExpression.Right))
                {
                    var shardingPredicateResult = IsShardingKey(binaryExpression.Left);
                    if (shardingPredicateResult.IsShardingKey)
                    {
                        conditionOnRight = true;
                        shardingPropertyName = shardingPredicateResult.ShardingPropertyName;
                        value = GetShardingKeyValue(binaryExpression.Right);
                    }
                    else
                        return x => true;
                }
                else if (IsConstantOrMember(binaryExpression.Left))
                {
                    var shardingPredicateResult = IsShardingKey(binaryExpression.Right);
                    if (shardingPredicateResult.IsShardingKey)
                    {
                        conditionOnRight = false;
                        shardingPropertyName = shardingPredicateResult.ShardingPropertyName;
                        value = GetShardingKeyValue(binaryExpression.Left);
                    }
                    else
                        return x => true;
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

                if (shardingPropertyName == null || value == default)
                    return x => true;


                return _keyToTailWithFilter(value, op, shardingPropertyName);
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