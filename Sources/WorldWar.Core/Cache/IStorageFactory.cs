using WorldWar.Core.Interfaces;

namespace WorldWar.Core.Cache
{
	public interface IStorageFactory
	{
		public IStorage<T> Create<T>();
	}
}