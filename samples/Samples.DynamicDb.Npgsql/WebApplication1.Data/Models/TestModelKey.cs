using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Data.Models
{

    public class TestModelKey
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Display(Name = "关键Key")]
        public string Key { get; set; } = "";

        [Display(Name = "创建日期")]
        public DateTime CreationDate { get; set; } = DateTime.Now;

    }
}
