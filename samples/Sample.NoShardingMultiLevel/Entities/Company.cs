namespace Sample.NoShardingMultiLevel.Entities
{
    public class Company
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BossId { get; set; }
        public virtual Boss Boss { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
    }
}
