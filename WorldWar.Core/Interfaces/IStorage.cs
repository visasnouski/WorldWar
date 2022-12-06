namespace WorldWar.Core.Interfaces
{
	public interface IStorage<T>
	{
		public bool TryGetValue(Guid id, out T? item);

		public void AddOrUpdate(Guid key, T item);

		public IEnumerable<T> Get();

		public void Remove(Guid key);

		public IEnumerable<T> GetByFilter(Func<T, bool> predicate);
	}
}
