using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Core.Cache
{
	public class CacheFactoryOptions
	{
		public IDictionary<string, Type> Types { get; } = new Dictionary<string, Type>();

		public void Register<T>()
			where T : IStorable
		{
			Types.Add(typeof(T).Name, typeof(Storage<T>));
		}
	}
}
