using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Core.Interfaces
{
	public interface IStorage<T>
	where T : IStorable
	{
		public T GetItem(Guid id);

		public void SetItem(T item);

		public IEnumerable<T> GetItems();

		public void RemoveItem(T item);

		public IEnumerable<T> GetVisibleItems(float latitude, float longitude, float viewingDistance);

		void SetItem(IReadOnlyCollection<T> items);
	}
}
