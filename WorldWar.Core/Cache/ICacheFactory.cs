using WorldWar.Core.Interfaces;

namespace WorldWar.Core.Cache
{
	public interface ICacheFactory
	{
		public IStorage<T> Create<T>();
	}
}