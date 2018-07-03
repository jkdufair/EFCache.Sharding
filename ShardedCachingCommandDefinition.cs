using System.Data.Common;
using System.Data.Entity.Core.Common;

namespace EFCache.Sharding
{
	public class ShardedCachingCommandDefinition : CachingCommandDefinition
	{
		private new readonly ShardedCacheTransactionHandler _cacheTransactionHandler;

		public ShardedCachingCommandDefinition(DbCommandDefinition commandDefinition, CommandTreeFacts commandTreeFacts,
			ShardedCacheTransactionHandler cacheTransactionHandler, CachingPolicy cachingPolicy) : base(
			commandDefinition,
			commandTreeFacts, cacheTransactionHandler, cachingPolicy)
		{
			_cacheTransactionHandler = cacheTransactionHandler;
		}

		public override DbCommand CreateCommand()
		{
			return new ShardedCachingCommand(_commandDefintion.CreateCommand(), _commandTreeFacts,
				_cacheTransactionHandler, _cachingPolicy);
		}
	}
}