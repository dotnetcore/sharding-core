using System;
using ShardingCore.Core;

namespace Sample.BulkConsole.Entities
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/7 21:08:43
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class Order
    {
        public string Id { get; set; }
        public string OrderNo { get; set; }
        public int Seq { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
