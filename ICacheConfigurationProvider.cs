using System.Data.Common;

namespace EFCache.Sharding
{
	public interface ICacheConfigurationProvider
	{
		RedisShardConfiguration GetConfiguration(DbConnection backingConnection);
	}
}