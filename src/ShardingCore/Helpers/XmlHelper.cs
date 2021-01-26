using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace ShardingCore.Helpers
{
    internal static class XmlHelper
    {
        /// <summary>
        /// 通过XML获取类属性注释
        /// 参考:https://github.com/dotnetcore/FreeSql/blob/d266446062b1dfcd694f7d213191cd2383410025/FreeSql/Internal/CommonUtils.cs
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>Dict：key=属性名，value=注释</returns>
        public static Dictionary<string, string> GetProperyCommentBySummary(Type type)
        {
            if (type.Assembly.IsDynamic) return null;
            //动态生成的程序集，访问不了 Assembly.Location/Assembly.CodeBase
            var regex = new Regex(@"\.(dll|exe)", RegexOptions.IgnoreCase);
            var xmlPath = regex.Replace(type.Assembly.Location, ".xml");
            if (File.Exists(xmlPath) == false)
            {
                if (string.IsNullOrEmpty(type.Assembly.CodeBase)) return null;
                xmlPath = regex.Replace(type.Assembly.CodeBase, ".xml");
                if (xmlPath.StartsWith("file:///") && Uri.TryCreate(xmlPath, UriKind.Absolute, out var tryuri))
                    xmlPath = tryuri.LocalPath;
                if (File.Exists(xmlPath) == false) return null;
            }

            var dic = new Dictionary<string, string>();
            var sReader = new StringReader(File.ReadAllText(xmlPath));
            using (var xmlReader = XmlReader.Create(sReader))
            {
                XPathDocument xpath = null;
                try
                {
                    xpath = new XPathDocument(xmlReader);
                }
                catch
                {
                    return null;
                }
                var xmlNav = xpath.CreateNavigator();

                var className = (type.IsNested ? $"{type.Namespace}.{type.DeclaringType.Name}.{type.Name}" : $"{type.Namespace}.{type.Name}").Trim('.');
                var node = xmlNav.SelectSingleNode($"/doc/members/member[@name='T:{className}']/summary");
                if (node != null)
                {
                    var comment = node.InnerXml.Trim(' ', '\r', '\n', '\t');
                    if (string.IsNullOrEmpty(comment) == false) dic.Add("", comment); //class注释
                }

                var props = type.GetPropertiesDictIgnoreCase().Values;
                foreach (var prop in props)
                {
                    className = (prop.DeclaringType.IsNested ? $"{prop.DeclaringType.Namespace}.{prop.DeclaringType.DeclaringType.Name}.{prop.DeclaringType.Name}" : $"{prop.DeclaringType.Namespace}.{prop.DeclaringType.Name}").Trim('.');
                    node = xmlNav.SelectSingleNode($"/doc/members/member[@name='P:{className}.{prop.Name}']/summary");
                    if (node == null) continue;
                    var comment = node.InnerXml.Trim(' ', '\r', '\n', '\t');
                    if (string.IsNullOrEmpty(comment)) continue;

                    dic.Add(prop.Name, comment);
                }
            }

            return dic;
        }
        private static ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _dicGetPropertiesDictIgnoreCase 
            = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();
        private static Dictionary<string, PropertyInfo> GetPropertiesDictIgnoreCase(this Type that) => that == null ? null : _dicGetPropertiesDictIgnoreCase.GetOrAdd(that, tp =>
        {
            var props = that.GetProperties().GroupBy(p => p.DeclaringType).Reverse().SelectMany(p => p); //将基类的属性位置放在前面 #164
            var dict = new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var prop in props)
            {
                if (dict.ContainsKey(prop.Name)) continue;
                dict.Add(prop.Name, prop);
            }
            return dict;
        });
    }
}
