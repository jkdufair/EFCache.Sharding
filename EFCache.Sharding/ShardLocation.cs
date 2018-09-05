namespace EFCache.Sharding
{
	/// <summary>
	/// This class exists as a POCO to convey shard information to the configurator
	/// It decouples this library from MS' shard library which is all sealed classes
	/// This allows our implementation to be more testable
	/// </summary>
	public class ShardLocation
	{
		public string Database { get; set; }
		public string Server { get; set; }
	}
}