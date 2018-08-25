using System;
using System.Collections.Generic;

namespace EFCache.Sharding
{
	public static class ShardedCacheConfigurator
	{
		/// <summary>
		/// Generate a lazy dictionary that maps shard database names to instances of <see cref="ICache"/>.
		/// Lazy because it uses EF and the call to the  DBConfiguration.Loaded method in <seealso cref="ShardedCacheTransactionHandler" />
		/// can't have had EF already initialized.
		/// </summary>
		/// <param name="handleError">The error handling function to get passed on to the shard's cache construction process</param>
		/// <param name="getConfiguredShards">Function that can be called to get a list of <see cref="Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.Shard" /> objects configured
		/// for the application</param>
		/// <param name="cacheFactory">Factory method for creating an <see cref="ICache" /> instance, given a <see cref="Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.Shard" /> and an
		/// func to call when there are caching exceptions</param>
		/// <returns>A dictionary representing a map from database names to <see cref="ICache"/> instances</returns>
		public static Lazy<Dictionary<string, ICache>> CreateShardDictionary(Action<Exception> handleError,
			Func<IEnumerable<ShardLocation>> getConfiguredShards, Func<ShardLocation, Action<Exception>, ICache> cacheFactory)
		{
			return new Lazy<Dictionary<string, ICache>>(() =>
			{
				var shardLocations = getConfiguredShards();
				var shardDict = new Dictionary<string, ICache>();
				foreach (var shardLocation in shardLocations)
				{
					var cache = cacheFactory(shardLocation, handleError);
					shardDict[shardLocation.Database] = cache;
				}

				return shardDict;
			});
		}
	}
}