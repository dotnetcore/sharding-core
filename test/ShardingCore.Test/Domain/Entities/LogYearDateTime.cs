using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShardingCore.Test.Domain.Entities
{
    public class LogYearDateTime
    {
        public Guid Id { get; set; }
        public string LogBody { get; set; }
        public DateTime LogTime { get; set; }
    }
}
