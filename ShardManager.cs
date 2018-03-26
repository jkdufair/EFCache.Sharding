using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;

namespace EFCache.Sharding
{
	public class ShardManager : ICache, IShardManager
	{
		private static readonly ConcurrentDictionary<string, ICache> Shards = new ConcurrentDictionary<string, ICache>();
		private static readonly ConcurrentDictionary<string, string> DatabaseCacheMap = new ConcurrentDictionary<string, string>();

		private readonly ICacheConfigurationProvider _cacheConfigurationProvider;
		private readonly ICacheFactory _cacheFactory;

		public ShardManager(ICacheConfigurationProvider cacheConfigurationProvider, ICacheFactory cacheFactory)
		{
			_cacheConfigurationProvider = cacheConfigurationProvider;
			_cacheFactory = cacheFactory;
		}

		public bool GetItem(string key, out object value, DbConnection backingConnection)
		{
			EnsureShard(backingConnection);
			return Shards[backingConnection.Database].GetItem(key, out value);
		}

		public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration, DateTimeOffset absoluteExpiration, DbConnection backingConnection)
		{
			EnsureShard(backingConnection);
			Shards[backingConnection.Database].PutItem(key, value, dependentEntitySets, slidingExpiration, absoluteExpiration);
		}

		public void InvalidateSets(IEnumerable<string> entitySets, DbConnection backingConnection)
		{
			EnsureShard(backingConnection);
			Shards[backingConnection.Database].InvalidateSets(entitySets);
		}

		public void InvalidateItem(string key, DbConnection backingConnection)
		{
			EnsureShard(backingConnection);
			Shards[backingConnection.Database].InvalidateItem(key);
		}

		public ICache GetShard(string databaseName)
		{
			return Shards[databaseName];
		}

		private void EnsureShard(DbConnection backingConnection)
		{
			if (DatabaseCacheMap.ContainsKey(backingConnection.Database)) return;
			var (connectionString, shouldCollectStatistics) = _cacheConfigurationProvider.GetConfiguration(backingConnection);
			// we don't check the results of TryAdd since we know the keys do not exist
			DatabaseCacheMap.TryAdd(backingConnection.Database, connectionString);
			Shards.TryAdd(backingConnection.Database, _cacheFactory.CreateCache(connectionString, shouldCollectStatistics));
		}
	}
}