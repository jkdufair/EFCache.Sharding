using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EFCache.Sharding
{
    public class ShardedCache : ICache
    {
        private readonly Func<string, string> _cacheConnectionStringProvider;
        private readonly Func<string, ICache> _cacheFactory;

        public ShardedCache(Func<string, string> cacheConnectionStringProvider, Func<string, ICache> cacheFactory)
        {
            _cacheConnectionStringProvider = cacheConnectionStringProvider;
            _cacheFactory = cacheFactory;
        }

        private readonly ConcurrentDictionary<string, ICache> _shards = new ConcurrentDictionary<string, ICache>();
        private readonly ConcurrentDictionary<string, string> _databaseCacheMap = new ConcurrentDictionary<string, string>();

        public bool GetItem(string key, out object value, string backingDatabaseName)
        {
            EnsureShard(backingDatabaseName);
            return _shards[backingDatabaseName].GetItem(key, out value);
        }

        public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration,
            DateTimeOffset absoluteExpiration, string backingDatabaseName)
        {
            EnsureShard(backingDatabaseName);
            _shards[backingDatabaseName].PutItem(key, value, dependentEntitySets, slidingExpiration, absoluteExpiration);
        }

        public void InvalidateSets(IEnumerable<string> entitySets, string backingDatabaseName)
        {
            EnsureShard(backingDatabaseName);
            _shards[backingDatabaseName].InvalidateSets(entitySets);
        }

        public void InvalidateItem(string key, string backingDatabaseName)
        {
            EnsureShard(backingDatabaseName);
            _shards[backingDatabaseName].InvalidateItem(key);
        }

        private void EnsureShard(string backingDatabaseName)
        {
            if (_databaseCacheMap.ContainsKey(backingDatabaseName)) return;
            var connectionString = _cacheConnectionStringProvider(backingDatabaseName);
            // we don't check the results since we know the keys do not exist
            _databaseCacheMap.TryAdd(backingDatabaseName, connectionString);
            _shards.TryAdd(backingDatabaseName, _cacheFactory(connectionString));
        }
    }
}