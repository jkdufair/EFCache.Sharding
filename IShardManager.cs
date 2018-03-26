namespace EFCache.Sharding
{
	public interface IShardManager
	{
		ICache GetShard(string databaseName);
	}
}