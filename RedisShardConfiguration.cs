namespace EFCache.Sharding
{
	public class RedisShardConfiguration
	{
		public string ConnectionString { get; set; }
		public bool ShouldCollectStatistics { get; set; }
	}
}