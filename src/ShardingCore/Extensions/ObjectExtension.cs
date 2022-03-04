using System;
using System.Reflection;

namespace ShardingCore.Extensions
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 16 December 2020 13:59:46
* @Email: 326308290@qq.com
*/
    public static class ObjectExtension
    {
        private static readonly BindingFlags _bindingFlags
            = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        
        /// <summary>
        /// 获取某字段值
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public static object GetTypeFieldValue(this Type type,object obj, string fieldName)
        {
            return type.GetField(fieldName, _bindingFlags).GetValue(obj);
        }
        /// <summary>
        /// 获取某字段值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public static object GetFieldValue(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, _bindingFlags).GetValue(obj);
        }
        /// <summary>
        /// 获取某属性值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName, _bindingFlags);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取某字段值
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static object GetTypePropertyValue(this Type type,object obj, string propertyName)
        {
            var property=type.GetProperty(propertyName, _bindingFlags);
            if (property != null)
            {
                return property.GetValue(obj);
            }
            else
            {
                return null;
            }
        }
        public static PropertyInfo GetObjectProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, _bindingFlags);
        }
        /// <summary>
        /// 类型X是否包含某个属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool ContainPropertyName(this Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName, _bindingFlags);
            return property != null;
        }



        public static Type GetGenericType0(this Type genericType,Type arg0Type)
        {
            return genericType.MakeGenericType(arg0Type);
        }
        public static Type GetGenericType1(this Type genericType,Type arg0Type, Type arg1Type)
        {
            return genericType.MakeGenericType(arg0Type, arg1Type);
        }

    }
}