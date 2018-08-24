using System;
using System.Collections.Generic;

namespace EFCache.Sharding
{
	/// <inheritdoc />
	/// <summary>
	/// This <see cref="T:EFCache.ICache" /> implementation can be used when caching is not desired. When building out
	/// a dictionary of caches per database shard, this should be used where caching is not configured for
	/// a database shard.
	/// Note: Because <see cref="M:EFCache.Sharding.NullCache.GetItem(System.String,System.Object@)" /> returns false, the out value is unused and none of the other
	/// methods are called
	/// </summary>
	public class NullCache : ICache
	{
		public bool GetItem(string key, out object value)
		{
			value = null;
			return false;
		}

		public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration,
			DateTimeOffset absoluteExpiration)
		{ }

		public void InvalidateSets(IEnumerable<string> entitySets)
		{ }

		public void InvalidateItem(string key)
		{ }
	}
}