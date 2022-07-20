using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.MySql.multi;
[Table("MyUser")]
public class MyUser
{
    public string Id { get; set; }
}