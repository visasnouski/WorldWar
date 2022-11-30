using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Core.Interfaces
{
	public interface IStorage<T>
	where T : IStorable
	{
		public T Get(Guid id);

		public void Set(T item);

		public IEnumerable<T> Get();

		public void Remove(T item);

		public IEnumerable<T> GetByFilter(Func<T, bool> predicate);

		void Set(IReadOnlyCollection<T> items);
	}
}
