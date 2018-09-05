using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EFCache.Sharding.Tests
{
	[TestClass]
	public class NullCacheTests
	{
		[TestMethod]
		public void GetItem_ShouldSetValueAtKeyToNullAndReturnFalseIndicatingValueNotCached()
		{
			var cache = new NullCache();
			var result = cache.GetItem("foo", out var value);
			Assert.AreEqual(value, null);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void GetItem_ShouldSetValueToNullForNullKeyAndReturnFalseIndicatingValueNotCached()
		{
			var cache = new NullCache();
			var result = cache.GetItem(null, out var value);
			Assert.AreEqual(value, null);
			Assert.IsFalse(result);
		}
	}
}