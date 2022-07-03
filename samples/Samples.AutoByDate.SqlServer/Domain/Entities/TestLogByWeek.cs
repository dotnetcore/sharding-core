using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShardingCore.Core;

namespace Samples.AutoByDate.SqlServer.Domain.Entities
{
    public class TestLogByWeek
    {
        public string Id { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
