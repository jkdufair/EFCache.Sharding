using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EFCache.Sharding
{
	/// <summary>
	/// Subclass of EFCache CacheTransactionHandler, overriding data access methods so the calls to
	/// the instance of <see cref="CacheTransactionHandler"/> can pass the database name to the sharded
	/// cache methods for dispatch to the proper cache instance.
	/// Note: most of these methods are effectively copy/paste of the superclass methods, sadly. Original
	/// maintainer rejected a simpler approach :-(
	/// </summary>
	public class ShardedCachingCommand : CachingCommand
	{
		private new readonly ShardedCacheTransactionHandler _cacheTransactionHandler;

		public ShardedCachingCommand(DbCommand command, CommandTreeFacts commandTreeFacts,
			ShardedCacheTransactionHandler cacheTransactionHandler, CachingPolicy cachingPolicy) : base(command,
			commandTreeFacts, cacheTransactionHandler, cachingPolicy)
		{
			_cacheTransactionHandler = cacheTransactionHandler;
		}

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			if (!IsCacheable)
			{
				var result = _command.ExecuteReader(behavior);

				if (!_commandTreeFacts.IsQuery)
				{
					_cacheTransactionHandler.InvalidateSets(Transaction,
						_commandTreeFacts.AffectedEntitySets.Select(s => s.Name), DbConnection.Database);
				}

				return result;
			}

			var key = CreateKey();

			if (_cacheTransactionHandler.GetItem(Transaction, key, out var value, DbConnection.Database))
			{
				return new CachingReader((CachedResults)value);
			}

			using (var reader = _command.ExecuteReader(behavior))
			{
				var queryResults = new List<object[]>();

				while (reader.Read())
				{
					var values = new object[reader.FieldCount];
					reader.GetValues(values);
					queryResults.Add(values);
				}

				return HandleCaching(reader, key, queryResults);
			}
		}

#if !NET40

		protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior,
			CancellationToken cancellationToken)
		{
			if (!IsCacheable)
			{
				var result = await _command.ExecuteReaderAsync(behavior, cancellationToken);

				if (!_commandTreeFacts.IsQuery)
				{
					_cacheTransactionHandler.InvalidateSets(Transaction,
						_commandTreeFacts.AffectedEntitySets.Select(s => s.Name), DbConnection.Database);
				}

				return result;
			}

			var key = CreateKey();

			object value;
			if (_cacheTransactionHandler.GetItem(Transaction, key, out value, DbConnection.Database))
			{
				return new CachingReader((CachedResults)value);
			}

			using (var reader = await _command.ExecuteReaderAsync(behavior, cancellationToken))
			{
				var queryResults = new List<object[]>();

				while (await reader.ReadAsync(cancellationToken))
				{
					var values = new object[reader.FieldCount];
					reader.GetValues(values);
					queryResults.Add(values);
				}

				return HandleCaching(reader, key, queryResults);
			}
		}

#endif

		protected override DbDataReader HandleCaching(DbDataReader reader, string key, List<object[]> queryResults)
		{
			var cachedResults =
				new CachedResults(
					GetTableMetadata(reader), queryResults, reader.RecordsAffected);

			_cachingPolicy.GetCacheableRows(_commandTreeFacts.AffectedEntitySets, out var minCacheableRows,
				out var maxCachableRows);

			if (IsQueryAlwaysCached ||
				(queryResults.Count >= minCacheableRows && queryResults.Count <= maxCachableRows))
			{
				_cachingPolicy.GetExpirationTimeout(_commandTreeFacts.AffectedEntitySets, out var slidingExpiration,
					out var absoluteExpiration);

				_cacheTransactionHandler.PutItem(
					Transaction,
					key,
					cachedResults,
					_commandTreeFacts.AffectedEntitySets.Select(s => s.Name),
					slidingExpiration,
					absoluteExpiration,
					DbConnection.Database);
			}

			return new CachingReader(cachedResults);
		}

		protected override void InvalidateSetsForNonQuery(int recordsAffected)
		{
			if (recordsAffected > 0 && _commandTreeFacts.AffectedEntitySets.Any())
			{
				_cacheTransactionHandler.InvalidateSets(Transaction,
					_commandTreeFacts.AffectedEntitySets.Select(s => s.Name), DbConnection.Database);
			}
		}

		public override object ExecuteScalar()
		{
			if (!IsCacheable)
			{
				return _command.ExecuteScalar();
			}

			var key = CreateKey();

			if (_cacheTransactionHandler.GetItem(Transaction, key, out var value, DbConnection.Database))
			{
				return value;
			}

			value = _command.ExecuteScalar();

			_cachingPolicy.GetExpirationTimeout(_commandTreeFacts.AffectedEntitySets, out var slidingExpiration,
				out var absoluteExpiration);

			_cacheTransactionHandler.PutItem(
				Transaction,
				key,
				value,
				_commandTreeFacts.AffectedEntitySets.Select(s => s.Name),
				slidingExpiration,
				absoluteExpiration,
				DbConnection.Database);

			return value;
		}

#if !NET40

		public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
		{
			if (!IsCacheable)
			{
				return await _command.ExecuteScalarAsync(cancellationToken);
			}

			var key = CreateKey();

			if (_cacheTransactionHandler.GetItem(Transaction, key, out var value, DbConnection.Database))
			{
				return value;
			}

			value = await _command.ExecuteScalarAsync(cancellationToken);

			_cachingPolicy.GetExpirationTimeout(_commandTreeFacts.AffectedEntitySets, out var slidingExpiration,
				out var absoluteExpiration);

			_cacheTransactionHandler.PutItem(
				Transaction,
				key,
				value,
				_commandTreeFacts.AffectedEntitySets.Select(s => s.Name),
				slidingExpiration,
				absoluteExpiration,
				DbConnection.Database);

			return value;
		}

#endif
	}
}