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
		/// Accepts a lazy dictionary mapping database names to instances of implementations of
		/// ICache
		/// </summary>
		/// <param name="shards"></param>
		public ShardedCacheTransactionHandler(Lazy<Dictionary<string, ICache>> shards)
		{
			_shards = shards;
		}

		protected override ICache ResolveCache(DbConnection dbConnection)
		{
			return _shards.Value[dbConnection.Database];
		}
	}
}