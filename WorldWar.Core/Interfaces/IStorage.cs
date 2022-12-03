namespace WorldWar.Core.Interfaces
{
	public interface IStorage<T>
	{
		public T Get(Guid id);

		public void Set(Guid key, T item);

		public IEnumerable<T> Get();

		public void Remove(Guid key);

		public IEnumerable<T> GetByFilter(Func<T, bool> predicate);
	}
}
