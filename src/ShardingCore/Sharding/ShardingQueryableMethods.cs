using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using ShardingCore.Core;

namespace ShardingCore.Sharding
{
    internal static class ShardingQueryableMethods
    {
        public static MethodInfo GetSumWithoutSelector(Type type)
        {
            Check.NotNull<Type>(type, nameof(type));
            return ShardingQueryableMethods.SumWithoutSelectorMethods[type];
        }

        private static Dictionary<Type, MethodInfo> AverageWithoutSelectorMethods { get; }

        private static Dictionary<Type, MethodInfo> AverageWithSelectorMethods { get; }

        private static Dictionary<Type, MethodInfo> SumWithoutSelectorMethods { get; }

        private static Dictionary<Type, MethodInfo> SumWithSelectorMethods { get; }
        public static MethodInfo AsQueryable { get; }
        public static MethodInfo LongCountWithoutPredicate { get; }
        public static MethodInfo Select { get; }

        static ShardingQueryableMethods()
        {
            Dictionary<string, List<MethodInfo>> queryableMethodGroups = ((IEnumerable<MethodInfo>)typeof(Queryable).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)).GroupBy<MethodInfo, string>((Func<MethodInfo, string>)(mi => mi.Name)).ToDictionary<IGrouping<string, MethodInfo>, string, List<MethodInfo>>((Func<IGrouping<string, MethodInfo>, string>)(e => e.Key), (Func<IGrouping<string, MethodInfo>, List<MethodInfo>>)(l => l.ToList<MethodInfo>()));
            Type[] typeArray = new Type[10]
            {
                typeof (int),
                typeof (int?),
                typeof (long),
                typeof (long?),
                typeof (float),
                typeof (float?),
                typeof (double),
                typeof (double?),
                typeof (Decimal),
                typeof (Decimal?)
            };
            ShardingQueryableMethods.AverageWithoutSelectorMethods = new Dictionary<Type, MethodInfo>();
            ShardingQueryableMethods.AverageWithSelectorMethods = new Dictionary<Type, MethodInfo>();
            ShardingQueryableMethods.SumWithoutSelectorMethods = new Dictionary<Type, MethodInfo>();
            ShardingQueryableMethods.SumWithSelectorMethods = new Dictionary<Type, MethodInfo>();
            ShardingQueryableMethods.AsQueryable = GetMethod(nameof(AsQueryable), 1, (Func<Type[], Type[]>)(types => new Type[1]
            {
                typeof (IEnumerable<>).MakeGenericType(types[0])
            }));

            ShardingQueryableMethods.LongCountWithoutPredicate = GetMethod("LongCount", 1, (Func<Type[], Type[]>)(types => new Type[1]
            {
                typeof (IQueryable<>).MakeGenericType(types[0])
            }));
            ShardingQueryableMethods.Select = GetMethod(nameof(Select), 2, (Func<Type[], Type[]>)(types => new Type[2]
            {
                typeof (IQueryable<>).MakeGenericType(types[0]),
                typeof (Expression<>).MakeGenericType(typeof (Func<,>).MakeGenericType(types[0], types[1]))
            }));
            foreach (Type type1 in typeArray)
            {
                Type type = type1;
                ShardingQueryableMethods.AverageWithoutSelectorMethods[type] = GetMethod("Average", 0, (Func<Type[], Type[]>)(types => new Type[1]
                {
                    typeof (IQueryable<>).MakeGenericType(type)
                }));
                ShardingQueryableMethods.AverageWithSelectorMethods[type] = GetMethod("Average", 1, (Func<Type[], Type[]>)(types => new Type[2]
                {
                    typeof (IQueryable<>).MakeGenericType(types[0]),
                    typeof (Expression<>).MakeGenericType(typeof (Func<,>).MakeGenericType(types[0], type))
                }));
                ShardingQueryableMethods.SumWithoutSelectorMethods[type] = GetMethod("Sum", 0, (Func<Type[], Type[]>)(types => new Type[1]
                {
                    typeof (IQueryable<>).MakeGenericType(type)
                }));
                ShardingQueryableMethods.SumWithSelectorMethods[type] = GetMethod("Sum", 1, (Func<Type[], Type[]>)(types => new Type[2]
                {
                    typeof (IQueryable<>).MakeGenericType(types[0]),
                    typeof (Expression<>).MakeGenericType(typeof (Func<,>).MakeGenericType(types[0], type))
                }));

            }
            MethodInfo GetMethod(
                string name,
                int genericParameterCount,
                Func<Type[], Type[]> parameterGenerator)
            {
                return queryableMethodGroups[name].Single<MethodInfo>((Func<MethodInfo, bool>)(mi => (genericParameterCount == 0 && !mi.IsGenericMethod || mi.IsGenericMethod && mi.GetGenericArguments().Length == genericParameterCount) && ((IEnumerable<ParameterInfo>)mi.GetParameters()).Select<ParameterInfo, Type>((Func<ParameterInfo, Type>)(e => e.ParameterType)).SequenceEqual<Type>((IEnumerable<Type>)parameterGenerator(mi.IsGenericMethod ? mi.GetGenericArguments() : Array.Empty<Type>()))));
            }
        }
    }
}
