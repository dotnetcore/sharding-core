using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Core.VirtualRoutes.TableRoutes
{
    public static class QueryResultCache
    {
        private static readonly ConcurrentDictionary<object, Func<string, bool>> _routeFilter =
            new ConcurrentDictionary<object, Func<string, bool>>();
        /// <summary>
        /// Returns the result of the query; if possible from the cache, otherwise
        /// the query is materialized and the result cached before being returned.
        /// </summary>
        public static Func<string, bool> FromCache(this Expression<Func<string,bool>> query)
        {
            string key = query.GetCacheKey();

            return _routeFilter.GetOrAdd(key, k => query.Compile());
        }

        /// <summary>
        /// Gets a cache key for a query.
        /// </summary>
        public static string GetCacheKey(this Expression expression)
        {

            // locally evaluate as much of the query as possible
            expression = Evaluator.PartialEval(expression, QueryResultCache.CanBeEvaluatedLocally);

            // support local collections
            expression = LocalCollectionExpander.Rewrite(expression);

            // use the string representation of the expression for the cache key
            string key = expression.ToString();

            // the key is potentially very long, so use an md5 fingerprint
            // (fine if the query result data isn't critically sensitive)
            key = key.ToMd5Fingerprint();

            return key;
        }

        static Func<Expression, bool> CanBeEvaluatedLocally
        {
            get
            {
                return expression =>
                {
                    // don't evaluate parameters
                    if (expression.NodeType == ExpressionType.Parameter)
                        return false;

                    // can't evaluate queries
                    if (typeof(IQueryable).IsAssignableFrom(expression.Type))
                        return false;

                    return true;
                };
            }
        }
    }

    /// <summary>
    /// Enables the partial evaluation of queries.
    /// </summary>
    /// <remarks>
    /// From http://msdn.microsoft.com/en-us/library/bb546158.aspx
    /// Copyright notice http://msdn.microsoft.com/en-gb/cc300389.aspx#O
    /// </remarks>
    public static class Evaluator
    {
        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        class SubtreeEvaluator : ExpressionVisitor
        {
            HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }

            internal Expression Eval(Expression exp)
            {
                return this.Visit(exp);
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                if (this.candidates.Contains(exp))
                {
                    return this.Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }
                LambdaExpression lambda = Expression.Lambda(e);
                Delegate fn = lambda.Compile();
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }

        }

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        class Nominator : ExpressionVisitor
        {
            Func<Expression, bool> fnCanBeEvaluated;
            HashSet<Expression> candidates;
            bool cannotBeEvaluated;

            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                this.candidates = new HashSet<Expression>();
                this.Visit(expression);
                return this.candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!this.cannotBeEvaluated)
                    {
                        if (this.fnCanBeEvaluated(expression))
                        {
                            this.candidates.Add(expression);
                        }
                        else
                        {
                            this.cannotBeEvaluated = true;
                        }
                    }
                    this.cannotBeEvaluated |= saveCannotBeEvaluated;
                }
                return expression;
            }
        }
    }

    /// <summary>
    /// Enables cache key support for local collection values.
    /// </summary>
    public class LocalCollectionExpander : ExpressionVisitor
    {
        public static Expression Rewrite(Expression expression)
        {
            return new LocalCollectionExpander().Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // pair the method's parameter types with its arguments
            var map = node.Method.GetParameters()
                .Zip(node.Arguments, (p, a) => new { Param = p.ParameterType, Arg = a })
                .ToLinkedList();

            // deal with instance methods
            var instanceType = node.Object == null ? null : node.Object.Type;
            map.AddFirst(new { Param = instanceType, Arg = node.Object });

            // for any local collection parameters in the method, make a
            // replacement argument which will print its elements
            var replacements = (from x in map
                                where x.Param != null && x.Param.IsGenericType
                                let g = x.Param.GetGenericTypeDefinition()
                                where g == typeof(IEnumerable<>) || g == typeof(List<>)
                                where x.Arg.NodeType == ExpressionType.Constant
                                let elementType = x.Param.GetGenericArguments().Single()
                                let printer = MakePrinter((ConstantExpression)x.Arg, elementType)
                                select new { x.Arg, Replacement = printer }).ToList();

            if (replacements.Any())
            {
                var args = map.Select(x => (from r in replacements
                                            where r.Arg == x.Arg
                                            select r.Replacement).SingleOrDefault() ?? x.Arg).ToList();

                node = node.Update(args.First(), args.Skip(1));
            }

            return base.VisitMethodCall(node);
        }

        ConstantExpression MakePrinter(ConstantExpression enumerable, Type elementType)
        {
            var value = (IEnumerable)enumerable.Value;
            var printerType = typeof(Printer<>).MakeGenericType(elementType);
            var printer = Activator.CreateInstance(printerType, value);

            return Expression.Constant(printer);
        }

        /// <summary>
        /// Overrides ToString to print each element of a collection.
        /// </summary>
        /// <remarks>
        /// Inherits List in order to support List.Contains instance method as well
        /// as standard Enumerable.Contains/Any extension methods.
        /// </remarks>
        class Printer<T> : List<T>
        {
            public Printer(IEnumerable collection)
            {
                this.AddRange(collection.Cast<T>());
            }

            public override string ToString()
            {
                return "{" + this.ToConcatenatedString(t => t.ToString(), "|") + "}";
            }
        }
    }

    public static class Utility
    {
        /// <summary>
        /// Creates an MD5 fingerprint of the string.
        /// </summary>
        public static string ToMd5Fingerprint(this string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s.ToCharArray());
            var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);

            // concat the hash bytes into one long string
            return hash.Aggregate(new StringBuilder(32),
                (sb, b) => sb.Append(b.ToString("X2")))
                .ToString();
        }

        public static string ToConcatenatedString<T>(this IEnumerable<T> source, Func<T, string> selector, string separator)
        {
            var b = new StringBuilder();
            bool needSeparator = false;

            foreach (var item in source)
            {
                if (needSeparator)
                    b.Append(separator);

                b.Append(selector(item));
                needSeparator = true;
            }

            return b.ToString();
        }

        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source)
        {
            return new LinkedList<T>(source);
        }
    }
}
