using System;
using System.Reflection;
using System.Threading.Tasks;
using ShardingCore.Jobs.Abstaractions;

namespace ShardingCore.Jobs.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 06 January 2021 13:08:55
* @Email: 326308290@qq.com
*/
    internal static class CommonExtension
    {
        
        public static bool IsJobType(this Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            return typeof(IJob).IsAssignableFrom(entityType);
        }
        public static bool IsAsyncMethod(this MethodInfo method)
        {
            return (typeof(Task).IsAssignableFrom(method.ReturnType));
        }
    }
}