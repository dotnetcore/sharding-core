using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data.Models
{
    public class TestModel
    {

        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        //public Guid Id2 { get; set; } = Guid.NewGuid();

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Content { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime CreationTime { get; set; } = DateTime.Now;

        public string TestNewField { get; set; } = "";

        public string AfterShardingDb { get; set; } = "";

    }
}
