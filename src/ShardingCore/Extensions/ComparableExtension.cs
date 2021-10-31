using System;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Sunday, 31 October 2021 15:41:12
* @Email: 326308290@qq.com
*/
    public static class ComparableExtension
    {
        public static int SafeCompareToWith(this IComparable value, IComparable other, bool asc)
        {
            if (asc)
                return SafeCompareTo(value, other);
            return SafeCompareTo(other, value);
        }
        public static int SafeCompareTo(IComparable value, IComparable other)
        {
            if (null == value && null == other) {
                return 0;
            }
            if (null == value)
            {
                return -1;
            }
            if (null == other) {
                return 1;
            }
            return value.CompareTo(other);
        }
    }
}