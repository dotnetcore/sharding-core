using System;
using System.Reflection;
using ShardingCore.Core;

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
            var property = obj.GetType().GetUltimateShadowingProperty(propertyName, _bindingFlags);
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
            var property=type.GetUltimateShadowingProperty(propertyName, _bindingFlags);
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
            return obj.GetType().GetUltimateShadowingProperty(propertyName,_bindingFlags);
        }
        /// <summary>
        /// 类型X是否包含某个属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool ContainPropertyName(this Type type, string propertyName)
        {
            var property = type.GetUltimateShadowingProperty(propertyName, _bindingFlags);
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
        public static PropertyInfo GetUltimateShadowingProperty(this Type type, string name)
        {

            return type.GetUltimateShadowingProperty(name,_bindingFlags);
        }
        /// <summary>
        /// https://github.com/nunit/nunit/blob/111fc6b5550f33b4fceb6ac8693c5692e99a5747/src/NUnitFramework/framework/Internal/Reflect.cs
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        public static PropertyInfo GetUltimateShadowingProperty(this Type type, string name, BindingFlags bindingFlags)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(name, nameof(name));
            if ((bindingFlags & BindingFlags.DeclaredOnly) != 0)
            {
                // If you're asking us to search a hierarchy but only want properties declared in the given type,
                // you're in the wrong place but okay:
                return type.GetProperty(name, bindingFlags);
            }

            if ((bindingFlags & (BindingFlags.Public | BindingFlags.NonPublic)) == (BindingFlags.Public | BindingFlags.NonPublic))
            {
                // If we're searching for both public and nonpublic properties, search for only public first
                // because chances are if there is a public property, it would be very surprising to detect the private shadowing property.

                for (var publicSearchType = type; publicSearchType != null; publicSearchType = publicSearchType.GetTypeInfo().BaseType)
                {
                    var property = publicSearchType.GetProperty(name, (bindingFlags | BindingFlags.DeclaredOnly) & ~BindingFlags.NonPublic);
                    if (property != null) return property;
                }

                // There is no public property, so may as well not ask to include them during the second search.
                bindingFlags &= ~BindingFlags.Public;
            }

            for (var searchType = type; searchType != null; searchType = searchType.GetTypeInfo().BaseType)
            {
                var property = searchType.GetProperty(name, bindingFlags | BindingFlags.DeclaredOnly);
                if (property != null) return property;
            }

            return null;
        }

    }
}