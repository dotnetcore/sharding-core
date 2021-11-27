using System;

namespace ShardingCore.Test2x.Domain.Entities
{
    public class LogDayLong
    {
        public Guid Id { get; set; }
        public string LogLevel { get; set; }
        public string LogBody { get; set; }
        public long LogTime { get; set; }
    }
}
