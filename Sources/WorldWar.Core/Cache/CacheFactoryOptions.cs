namespace WorldWar.Core.Cache
{
	public class CacheFactoryOptions
	{
		public IDictionary<string, Type> Types { get; } = new Dictionary<string, Type>();

		public void Register<T>()
		{
			Types.Add(typeof(T).Name, typeof(Storage<T>));
		}
	}
}
