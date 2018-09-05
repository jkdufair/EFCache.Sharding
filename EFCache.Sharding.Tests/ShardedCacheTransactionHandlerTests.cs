using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace EFCache.Sharding.Tests
{
	[TestClass]
	public class ShardedCacheTransactionHandlerTests
	{
		[TestMethod]
		public void ShardedCacheTransactionHandler_ShouldResolveICache()
		{
			const string dbName = "some-database";
			var applicationCache = new NullCache();

			var dbConnectionMock = new Mock<DbConnection>();
			dbConnectionMock.Setup(conn => conn.Database).Returns(dbName);

			var shardDictionary = new Lazy<Dictionary<string, ICache>>();
			shardDictionary.Value[dbName] = applicationCache;

			var handler = new ShardedCacheTransactionHandlerTester(shardDictionary);
			var someOtherCache = new NullCache();
			var resolvedCache = handler.TestCacheResolver(dbConnectionMock.Object);

			Assert.AreSame(resolvedCache, applicationCache);
			Assert.AreNotSame(resolvedCache, someOtherCache);
		}

		[ExpectedException(typeof(KeyNotFoundException))]
		[TestMethod]
		public void ShardedCacheTransactionHandler_ShouldThrowIfDictionaryEmpty()
		{
			const string dbName = "some-database";

			var dbConnectionMock = new Mock<DbConnection>();
			dbConnectionMock.Setup(conn => conn.Database).Returns(dbName);

			var shardDictionary = new Lazy<Dictionary<string, ICache>>();

			var handler = new ShardedCacheTransactionHandlerTester(shardDictionary);
			handler.TestCacheResolver(dbConnectionMock.Object);
		}
	}

	public class ShardedCacheTransactionHandlerTester : ShardedCacheTransactionHandler
	{
		public ShardedCacheTransactionHandlerTester(Lazy<Dictionary<string, ICache>> shards) : base(shards)
		{ }

		public ICache TestCacheResolver(DbConnection dbConnection) => ResolveCache(dbConnection);
	}
}