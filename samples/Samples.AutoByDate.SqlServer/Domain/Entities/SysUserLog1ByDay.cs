using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Samples.AutoByDate.SqlServer.Domain.Entities
{
    public class SysUserLog1ByDay
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime CreateTime { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public  virtual SysUserLogByDay SysUserLog { get; set; }
    }
}
