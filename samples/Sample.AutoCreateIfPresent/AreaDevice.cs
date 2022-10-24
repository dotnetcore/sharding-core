using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.AutoCreateIfPresent
{
    [Table("aaa")]
    public class AreaDevice
    {
        public string Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string Area { get; set; }
    }
}
