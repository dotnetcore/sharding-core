using System;

namespace WebApplication1.Data.Models
{
    public class TestModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Content { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime CreationTime { get; set; } = DateTime.Now;

        public string TestNewField { get; set; } = "";

        public string AfterShardingDb { get; set; } = "";

    }
}
