using System;

namespace EFCache.Sharding
{
    public interface ICacheFactory
    {
        ICache CreateCache(string connectionString, bool shouldCollectStatistics);
    }
}