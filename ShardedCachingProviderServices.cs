using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCache.Sharding
{
	public class ShardedCachingProviderServices : CachingProviderServices
	{
		private new readonly ShardedCacheTransactionHandler _cacheTransactionHandler;

		public ShardedCachingProviderServices(DbProviderServices providerServices, ShardedCacheTransactionHandler cacheTransactionHandler, CachingPolicy cachingPolicy = null) : base(providerServices, cacheTransactionHandler, cachingPolicy)
		{
			_cacheTransactionHandler = cacheTransactionHandler;
		}

		protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
		{
			return new ShardedCachingCommandDefinition(
				_providerServices.CreateCommandDefinition(providerManifest, commandTree),
				new CommandTreeFacts(commandTree),
				_cacheTransactionHandler,
				_cachingPolicy);
		}
	}
}