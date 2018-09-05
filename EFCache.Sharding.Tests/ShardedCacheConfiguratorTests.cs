using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCache.Sharding.Tests
{
	[TestClass]
	public class ShardedCacheConfiguratorTests
	{
		[TestMethod]
		public void CreateShardDictionary_ShouldCreateDictionaryWithErrorHandler()
		{
			const string databaseName = "some-database";
			var applicationCache = new NullCache();
			var result = ShardedCacheConfigurator.CreateShardDictionary(() => new List<ShardLocation> { new ShardLocation { Database = databaseName, Server = "unused-in-tests" } },
				(shard, handler) => applicationCache, exception => { }).Value;
			var expectedResult = new Lazy<Dictionary<string, ICache>>();
			expectedResult.Value[databaseName] = applicationCache;
			var someOtherCache = new NullCache();
			Assert.AreEqual(1, result.Keys.Count);
			Assert.AreEqual(databaseName, result.Keys.ElementAt(0));
			Assert.AreSame(applicationCache, result.Values.ElementAt(0));
			Assert.AreNotSame(someOtherCache, result.Values.ElementAt(0));
		}

		[TestMethod]
		public void CreateShardDictionary_ShouldCreateDictionaryWithoutErrorHandler()
		{
			const string databaseName = "some-database";
			var applicationCache = new NullCache();
			var result = ShardedCacheConfigurator.CreateShardDictionary(() => new List<ShardLocation> { new ShardLocation { Database = databaseName, Server = "unused-in-tests" } },
				(shard, handler) => applicationCache, null).Value;
			var expectedResult = new Lazy<Dictionary<string, ICache>>();
			expectedResult.Value[databaseName] = applicationCache;
			var someOtherCache = new NullCache();
			Assert.AreEqual(1, result.Keys.Count);
			Assert.AreEqual(databaseName, result.Keys.ElementAt(0));
			Assert.AreSame(applicationCache, result.Values.ElementAt(0));
			Assert.AreNotSame(someOtherCache, result.Values.ElementAt(0));
		}

		[ExpectedException(typeof(NullReferenceException))]
		[TestMethod]
		public void CreateShardDictionary_ShouldThrowWithoutDatabases()
		{
			var applicationCache = new NullCache();
			var result = ShardedCacheConfigurator.CreateShardDictionary(null, (shard, handler) => applicationCache, null).Value;
		}

		[ExpectedException(typeof(NullReferenceException))]
		[TestMethod]
		public void CreateShardDictionary_ShouldThrowWithoutCacheFactory()
		{
			var result = ShardedCacheConfigurator.CreateShardDictionary(() => new List<ShardLocation> { new ShardLocation { Database = "unused", Server = "unused-in-tests" } }, null, null).Value;
		}
	}
}