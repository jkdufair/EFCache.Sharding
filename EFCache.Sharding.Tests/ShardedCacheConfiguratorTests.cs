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
			var result = ShardedCacheConfigurator.CreateShardDictionary(exception => { },
				() => new List<ShardLocation> { new ShardLocation { Database = databaseName, Server = "unused-in-tests" } },
				(shard, handler) => applicationCache).Value;
			var expectedResult = new Lazy<Dictionary<string, ICache>>();
			expectedResult.Value[databaseName] = applicationCache;
			var someOtherCache = new NullCache();
			Assert.AreEqual(result.Keys.Count, 1);
			Assert.AreEqual(result.Keys.ElementAt(0), databaseName);
			Assert.AreSame(result.Values.ElementAt(0), applicationCache);
			Assert.AreNotSame(result.Values.ElementAt(0), someOtherCache);
		}

		[TestMethod]
		public void CreateShardDictionary_ShouldCreateDictionaryWithoutErrorHandler()
		{
			const string databaseName = "some-database";
			var applicationCache = new NullCache();
			var result = ShardedCacheConfigurator.CreateShardDictionary(null,
				() => new List<ShardLocation> { new ShardLocation { Database = databaseName, Server = "unused-in-tests" } },
				(shard, handler) => applicationCache).Value;
			var expectedResult = new Lazy<Dictionary<string, ICache>>();
			expectedResult.Value[databaseName] = applicationCache;
			var someOtherCache = new NullCache();
			Assert.AreEqual(result.Keys.Count, 1);
			Assert.AreEqual(result.Keys.ElementAt(0), databaseName);
			Assert.AreSame(result.Values.ElementAt(0), applicationCache);
			Assert.AreNotSame(result.Values.ElementAt(0), someOtherCache);
		}

		[ExpectedException(typeof(NullReferenceException))]
		[TestMethod]
		public void CreateShardDictionary_ShouldThrowWithoutDatabases()
		{
			var applicationCache = new NullCache();
			var result = ShardedCacheConfigurator.CreateShardDictionary(null, null, (shard, handler) => applicationCache).Value;
		}

		[ExpectedException(typeof(NullReferenceException))]
		[TestMethod]
		public void CreateShardDictionary_ShouldThrowWithoutCacheFactory()
		{
			var result = ShardedCacheConfigurator.CreateShardDictionary(null,
				() => new List<ShardLocation> { new ShardLocation { Database = "unused", Server = "unused-in-tests" } }, null).Value;
		}
	}
}