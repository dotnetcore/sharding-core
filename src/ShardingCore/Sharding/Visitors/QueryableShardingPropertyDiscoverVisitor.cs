//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using ShardingCore.Core.EntityMetadatas;
//using ShardingCore.Core.VirtualRoutes;
//using ShardingCore.Exceptions;
//using ShardingCore.Extensions;

//namespace ShardingCore.Sharding.Visitors
//{
//    internal class QueryableShardingPropertyDiscoverVisitor:ExpressionVisitor
//    {
//        private readonly IDictionary<Type, EntityMetadata> _queryEntities;
//        private readonly Dictionary<Type,ISet<string>> _queryProperties;

//        public QueryableShardingPropertyDiscoverVisitor(IDictionary<Type,EntityMetadata> queryEntities)
//        {
//            _queryEntities = queryEntities;
//            _queryProperties = new Dictionary<Type, ISet<string>>();
//        }

//        public Dictionary<Type, ISet<string>> GetQueryProperties()
//        {
//            return _queryProperties;
//        }

//        private bool IsShardingKey(Expression expression)
//        {
//            if (expression is MemberExpression member)
//            {
//                if (_queryEntities.TryGetValue(member.Expression.Type, out var entityMetadata))
//                {
//                    //是否是分片字段
//                    if (entityMetadata.ShardingDataSourceProperties.ContainsKey(member.Member.Name) ||
//                        entityMetadata.ShardingTableProperties.ContainsKey(member.Member.Name))
//                    {
//                        if (!_queryProperties.TryGetValue(member.Expression.Type,out var propertyInfos))
//                        {
//                            propertyInfos = new HashSet<string>();
//                            _queryProperties.Add(member.Expression.Type, propertyInfos);
//                        }

//                        propertyInfos.Add(member.Member.Name);
//                        return true;
//                    }
//                }
//            }
//            return false;
//        }
//        /// <summary>
//        /// 方法是否包含shardingKey xxx.invoke(shardingkey) eg. <code>o=>new[]{}.Contains(o.Id)</code>
//        /// </summary>
//        /// <param name="methodCallExpression"></param>
//        /// <returns></returns>
//        private bool IsMethodWrapShardingKey(MethodCallExpression methodCallExpression)
//        {
//            if (methodCallExpression.Arguments.IsNotEmpty())
//            {
//                for (int i = 0; i < methodCallExpression.Arguments.Count; i++)
//                {
//                    var isShardingKey = IsShardingKey(methodCallExpression.Arguments[i]);
//                    if (isShardingKey) return true;
//                }
//            }

//            return false;
//        }

//        private bool IsShardingWrapConstant(MethodCallExpression methodCallExpression)
//        {
//            return methodCallExpression.Object != null && IsShardingKey(methodCallExpression.Object);
//        }
//        private bool IsConstantOrMember(Expression expression)
//        {
//            return expression is ConstantExpression
//                   || (expression is MemberExpression member && (member.Expression is ConstantExpression || member.Expression is MemberExpression || member.Expression is MemberExpression))
//                   || expression is MethodCallExpression;
//        }

//        private bool IsMethodCall(Expression expression)
//        {
//            return expression is MethodCallExpression;
//        }

//        protected override Expression VisitMethodCall(MethodCallExpression node)
//        {
//            if (node.Method.Name == nameof(Queryable.Where))
//            {
//                if (node.Arguments[1] is UnaryExpression unaryExpression)
//                {
//                    if (unaryExpression.Operand is LambdaExpression lambdaExpression)
//                    {
//                        Resolve(lambdaExpression);
//                    }
//                }
//            }

//            return base.VisitMethodCall(node);
//        }


//        private Expression<Func<string, bool>> Resolve(Expression expression)
//        {
//            if (expression is LambdaExpression lambda)
//            {
//                expression = lambda.Body;
//                return Resolve(expression);
//            }

//            if (expression is BinaryExpression binaryExpression) //解析二元运算符
//            {
//                return ParseGetWhere(binaryExpression);
//            }

//            if (expression is UnaryExpression unary) //解析一元运算符
//            {
//                if (unary.Operand is MethodCallExpression unaryCallExpression)
//                {
//                    // return ResolveLinqToObject(unary.Operand, false);
//                    return ResolveInFunc(unaryCallExpression, unary.NodeType != ExpressionType.Not);
//                }
//            }

//            if (expression is MethodCallExpression methodCallExpression) //解析扩展方法
//            {
//                return ResolveInFunc(methodCallExpression, true);
//            }
//            return o => true;
//        }

//        private Expression<Func<string, bool>> ResolveInFunc(MethodCallExpression methodCallExpression, bool @in)
//        {
//            if (methodCallExpression.IsEnumerableContains(methodCallExpression.Method.Name))
//            {
//                IsMethodWrapShardingKey(methodCallExpression);
//            }

//            if (methodCallExpression.IsNamedEquals())
//            {
//                //"".equals(o.id)
//                if (IsMethodWrapShardingKey(methodCallExpression))
//                {
//                }
//                //o.id.equals("")
//                else if (IsShardingWrapConstant(methodCallExpression))
//                {
//                }
//            }

//            return x => true;
//        }

//        private Expression<Func<string, bool>> ParseGetWhere(BinaryExpression binaryExpression)
//        {
//            Expression<Func<string, bool>> left = x => true;
//            Expression<Func<string, bool>> right = x => true;

//            //递归获取
//            if (binaryExpression.Left is BinaryExpression)
//                left = ParseGetWhere(binaryExpression.Left as BinaryExpression);
//            if (binaryExpression.Left is MethodCallExpression methodCallExpression)
//                left = Resolve(methodCallExpression);

//            if (binaryExpression.Left is UnaryExpression unaryExpression)
//                left = Resolve(unaryExpression);

//            if (binaryExpression.Right is BinaryExpression)
//                right = ParseGetWhere(binaryExpression.Right as BinaryExpression);

//            //组合
//            if (binaryExpression.NodeType == ExpressionType.AndAlso)
//            {
//                return left.And(right);
//            }
//            else if (binaryExpression.NodeType == ExpressionType.OrElse)
//            {
//                return left.Or(right);
//            }
//            //单个
//            else
//            {
//                //条件在右边

//                if (IsConstantOrMember(binaryExpression.Right)&&IsShardingKey(binaryExpression.Left))
//                {
//                }
//                else if (IsConstantOrMember(binaryExpression.Left) && IsShardingKey(binaryExpression.Right))
//                {
//                }


//                return x => true;
//            }
//        }
//    }
//}
