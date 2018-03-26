namespace EFCache.Sharding
{
	public interface IShardedCache
	{
		ICache GetShard(string databaseName);
	}
}