namespace EFCache.Sharding
{
	public interface IShardedCache : ICache
	{
		ICache GetShard(string databaseName);
	}
}