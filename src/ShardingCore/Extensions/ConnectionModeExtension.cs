//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using ShardingCore.Core;

//namespace ShardingCore.Extensions
//{
//    internal static class ConnectionModeExtension
//    {
//        public static async Task<T> ReleaseConnectionAsync<T>(this Task<T> executeTask, DbContext dbContext,
//            ConnectionModeEnum connectionMode)
//        {
//            try
//            {
//                return await executeTask;
//            }
//            finally
//            {
////                if (connectionMode == ConnectionModeEnum.CONNECTION_STRICTLY)
////                {
////#if !EFCORE2
////                    await dbContext.DisposeAsync();
////#endif
////#if EFCORE2
////                    dbContext.Dispose();
////#endif
////                }
//            }
//        }
//    }
//}
