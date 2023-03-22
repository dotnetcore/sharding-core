using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ShardingCore.Extensions.InternalExtensions
{
    internal static class InternalIEntityTypeExtension
    {
        /// <summary>
        /// 获取在db context内的数据库表名称对应叫什么
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static string GetEntityTypeTableName(this IEntityType entityType)
        {
#if !EFCORE2
            var tableName = entityType.GetTableName();
#endif
#if EFCORE2
            var tableName = entityType.Relational().TableName;
#endif
            return tableName;
        }
        public static string GetEntityTypeSchema(this IEntityType entityType)
        {
#if !EFCORE2
            var tableName = entityType.GetSchema();
#endif
#if EFCORE2
            var tableName = entityType.Relational().Schema;
#endif
            return tableName;
        }

        public static bool GetEntityTypeIsView(this IEntityType entityType)
        {
#if !EFCORE2&&!EFCORE3
            return !string.IsNullOrWhiteSpace(entityType.GetViewName());
#endif
#if EFCORE2 ||EFCORE3
            return false;
#endif
        }
        public static string GetEntityTypeViewName(this IEntityType entityType)
        {
#if !EFCORE2&&!EFCORE3
            return entityType.GetViewName();
#endif
#if EFCORE2 ||EFCORE3
            return null;
#endif
        }
        public static string GetEntityTypeViewSchema(this IEntityType entityType)
        {
#if !EFCORE2&&!EFCORE3
            return entityType.GetViewSchema();
#endif
#if EFCORE2 ||EFCORE3
            return null;
#endif
        }
    }
}
