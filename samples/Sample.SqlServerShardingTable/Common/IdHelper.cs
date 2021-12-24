using System;

namespace Sample.SqlServerShardingTable.Common
{
    public class IdHelper
    {
        private IdHelper(){}
        private static SnowflakeId _snowflakeId;

         static IdHelper()
         {
             _snowflakeId = new SnowflakeId(1,3);
         }

         public static long NextId()
         {
             return _snowflakeId.NextId();
         }

         public static DateTime FromId(long id)
         {
             return SnowflakeId.AnalyzeIdToDateTime(id);
         }
    }
}
