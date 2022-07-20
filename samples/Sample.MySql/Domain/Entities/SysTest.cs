namespace Sample.MySql.Domain.Entities
{
    public interface IUser
    {
        string UserId { get; set; }
    }
    public class SysTest:IUser
    {
        public string Id { get; set; }
        public string UserId { get; set; }
    }
}