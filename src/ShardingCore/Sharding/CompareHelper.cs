using System;

namespace ShardingCore.Core.Internal.StreamMerge
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 25 January 2021 12:48:41
* @Email: 326308290@qq.com
*/
    internal class CompareHelper
    {
        private CompareHelper(){}

        public static int CompareTo(IComparable value, IComparable other)
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

        public static int CompareToWith(IComparable value, IComparable other,bool asc)
        {
            if (asc)
                return CompareTo(value, other);
            return CompareTo(other, value);
        }
    }
}