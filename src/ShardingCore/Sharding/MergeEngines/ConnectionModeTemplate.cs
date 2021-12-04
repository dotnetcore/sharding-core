//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using ShardingCore.Core;

//namespace ShardingCore.Sharding.MergeEngines
//{
//    internal class ConnectionModeTemplate
//    {
//        [ExcludeFromCodeCoverage]
//        private ConnectionModeTemplate() { }

//        public static async Task<T> ExecuteAsync<T>(Func<Task<T>> func, DbContext dbContext, ConnectionModeEnum connectionMode)
//        {
//            try
//            {
//                return await func();
//            }
//            finally
//            {
//                if (connectionMode == ConnectionModeEnum.IN_MEMORY_MERGE)
//                {
//#if !EFCORE
//                    await dbContext.DisposeAsync();
//#endif
//#if EFCORE2
//                    dbContext.Dispose();
//#endif
//                }
//            }
//        }
//    }
//}
