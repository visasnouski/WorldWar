using System.Collections.Concurrent;
using WorldWar.Abstractions.Exceptions;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

public class Storage<T> : IStorage<T>
	where T : IStorable
{
	private static readonly ConcurrentDictionary<Guid, T> ItemsStorage = new();

	public T Get(Guid id)
	{
		if (ItemsStorage.TryGetValue(id, out var item))
		{
			return item;
		}

		throw new ItemNotFoundException("item not found");
	}

	public void Set(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException(nameof(item));
		}

		ItemsStorage.AddOrUpdate(item.Id, item, (_, _) => item);
	}

	public IEnumerable<T> Get()
	{
		return ItemsStorage.Values.Select(x => x);
	}

	public void Remove(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException(nameof(item));
		}

		ItemsStorage.TryRemove(item.Id, out _);
	}

	public IEnumerable<T> GetByFilter(Func<T,bool> predicate)
	{
		return ItemsStorage.Values.Where(predicate);
	}

	public void Set(IReadOnlyCollection<T> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException(nameof(items));
		}

		foreach (var item in items)
		{
			Set(item);
		}
	}
}