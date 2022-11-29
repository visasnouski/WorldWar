using System.Collections.Concurrent;
using System.Numerics;
using WorldWar.Abstractions.Exceptions;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

public class Storage<T> : IStorage<T>
	where T : IStorable
{
	private static readonly ConcurrentDictionary<Guid, T> ItemsStorage = new();

	public T GetItem(Guid id)
	{
		if (ItemsStorage.TryGetValue(id, out var item))
		{
			return item;
		}

		throw new ItemNotFoundException("item not found");
	}

	public void SetItem(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException(nameof(item));
		}

		ItemsStorage.AddOrUpdate(item.Id, item, (_, _) => item);
	}

	public IEnumerable<T> GetItems()
	{
		return ItemsStorage.Values.Select(x => x);
	}

	public void RemoveItem(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException(nameof(item));
		}

		ItemsStorage.TryRemove(item.Id, out _);
	}

	public IEnumerable<T> GetVisibleItems(float latitude, float longitude, float viewingDistance)
	{
		return ItemsStorage.Values.Where(item =>
			CanSee(latitude, longitude, item.Latitude, item.Longitude, viewingDistance));
	}

	public void SetItem(IReadOnlyCollection<T> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException(nameof(items));
		}

		foreach (var item in items)
		{
			SetItem(item);
		}
	}


	private static bool CanSee(float centerLatitude, float centerLongitude, float latitude, float longitude, float viewingDistance)
	{
		return Vector2.Distance(new Vector2(centerLongitude, centerLatitude), new Vector2(longitude, latitude)) < viewingDistance;
	}
}