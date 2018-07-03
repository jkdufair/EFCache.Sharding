using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EFCache.Sharding
{
	/// <summary>
	/// Acts as a dispatcher to one or more <see cref="ICache" /> instances, mapped 1:1
	/// to names of databases
	/// </summary>
	public class ShardedCacheTransactionHandler : CacheTransactionHandler
	{
		private static Lazy<Dictionary<string, ICache>> _shards;

		/// <summary>
		/// Implemented for compatibility purposes
		/// </summary>
		/// <param name="cache"></param>
		public ShardedCacheTransactionHandler(ICache cache) : base(cache)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Accepts a lazy dictionary mapping database names to instances of implementations of
		/// ICache
		/// </summary>
		/// <param name="shards"></param>
		public ShardedCacheTransactionHandler(Lazy<Dictionary<string, ICache>> shards)
		{
			_shards = shards;
		}

		public static bool ShouldBypass { get; set; } = false;

		public bool GetItem(DbTransaction transaction, string key, out object value, string databaseName)
		{
			if (ShouldBypass)
			{
				value = null;
				return false;
			}

			return _shards.Value[databaseName].GetItem(key, out value);
		}

		public void PutItem(DbTransaction transaction, string key, object value,
			IEnumerable<string> dependentEntitySets,
			TimeSpan slidingExpiration, DateTimeOffset absoluteExpiration, string databaseName)
		{
			if (ShouldBypass) return;
			_shards.Value[databaseName].PutItem(key, value, dependentEntitySets, slidingExpiration, absoluteExpiration);
		}

		public void InvalidateSets(DbTransaction transaction, IEnumerable<string> entitySets,
			string databaseName)
		{
			if (ShouldBypass) return;
			_shards.Value[databaseName].InvalidateSets(entitySets);
		}

		public ICache GetShard(string databaseName)
		{
			return ShouldBypass ? null : _shards.Value[databaseName];
		}
	}
}