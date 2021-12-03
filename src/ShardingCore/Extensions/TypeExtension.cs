using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Extensions
{
    internal static class TypeExtension
    {
        /// <summary>
        /// 判断类型是否是可为空类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableType(this Type type)
        {
            return !type.IsValueType || (Nullable.GetUnderlyingType(type) != null);
        }
        /// <summary>
        /// 检测是否是数字类型,包括nullable的数字类型
        /// </summary>
        /// <remarks>
        /// bool 不是数字类型
        /// </remarks>
        public static bool IsNumericType(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }
        /// <summary>
        /// 是否是简单类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsSimpleType(this object obj)
        {
            if(null==obj)
                return false;
            return obj.GetType().IsSimpleType();
        }
        /// <summary>
        /// 是否是简单类型
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsSimpleType(this Type t)
        {
            return t.IsPrimitive || t.IsValueType || (t == typeof(string));
        }
        /// <summary>
        /// 是否是bool类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBooleanType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return Type.GetTypeCode(type) == TypeCode.Boolean;
        }
    }
}
