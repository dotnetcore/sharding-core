using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.MySql.Domain.Entities;

[Table("DynamicTable")]
public class DynamicTable
{
    public string Id { get; set; }
}