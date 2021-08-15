using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace ShardingCore.Sharding.Internal
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/15 8:50:09
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    internal static class NonCapturingLazyInitializer
    {
        public static TValue EnsureInitialized<TParam, TValue>(
             ref TValue target,
            TParam param,
            [NotNull] Func<TParam, TValue> valueFactory)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                return tmp;
            }

            Interlocked.CompareExchange(ref target, valueFactory(param), null);

            return target!;
        }

        public static TValue EnsureInitialized<TParam1, TParam2, TValue>(
             ref TValue target,
             TParam1 param1,
             TParam2 param2,
            Func<TParam1, TParam2, TValue> valueFactory)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                return tmp;
            }

            Interlocked.CompareExchange(ref target, valueFactory(param1, param2), null);

            return target!;
        }

        public static TValue EnsureInitialized<TValue>(
            ref TValue target,
            TValue value)
            where TValue : class
        {
            var tmp = Volatile.Read(ref target);
            if (tmp != null)
            {
                return tmp;
            }

            Interlocked.CompareExchange(ref target, value, null);

            return target!;
        }

        public static TValue EnsureInitialized<TParam, TValue>(
            ref TValue target,
            TParam param,
            Action<TParam> valueFactory)
            where TValue : class
        {
            if (Volatile.Read(ref target) != null)
            {
                return target!;
            }

            valueFactory(param);

            return Volatile.Read(ref target)!;
        }
    }
}
