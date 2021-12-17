using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShardingCore.Extensions
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/8/17 9:43:00
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public static class GenericExtension
    {
        public static Type[] GetGenericArguments(this Type type, Type genericType)
        {
            return type.GetInterfaces() //取类型的接口
                .Where(i => IsGenericType(i)) //筛选出相应泛型接口
                .SelectMany(i => i.GetGenericArguments()) //选择所有接口的泛型参数
                .ToArray(); //ToArray

            bool IsGenericType(Type type1)
                => type1.IsGenericType && type1.GetGenericTypeDefinition() == genericType;
        }
        public static bool HasImplementedRawGeneric(this Type type, Type generic)

        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (generic == null) throw new ArgumentNullException(nameof(generic));

            // 测试接口。

            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);

            if (isTheRawGenericType) return true;

            // 测试类型。

            while (type != null && type != typeof(object))

            {
                isTheRawGenericType = IsTheRawGenericType(type);

                if (isTheRawGenericType) return true;

                type = type.BaseType;

            }

            // 没有找到任何匹配的接口或类型。

            return false;

            // 测试某个类型是否是指定的原始接口。

            bool IsTheRawGenericType(Type test)

                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);

        }
    }
}
