namespace Sample.NoShardingMultiLevel.Entities
{
    public class Boss
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual Company Company { get; set; }
    }
}
