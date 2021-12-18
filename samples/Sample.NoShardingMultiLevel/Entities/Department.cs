namespace Sample.NoShardingMultiLevel.Entities
{
    public class Department
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public virtual Company Company { get; set; }
    }
}
