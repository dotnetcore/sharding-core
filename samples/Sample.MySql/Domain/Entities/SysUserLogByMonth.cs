using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShardingCore.Core;

namespace Sample.MySql.Domain.Entities
{
    public class SysUserLogByMonth 
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
    }
}
