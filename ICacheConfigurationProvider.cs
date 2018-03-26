using System.Data.Common;

namespace EFCache.Sharding
{
	public interface ICacheConfigurationProvider
	{
		(string, bool) GetConfiguration(DbConnection backingConnection);
	}
}