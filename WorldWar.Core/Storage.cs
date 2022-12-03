﻿using System.Collections.Concurrent;
using WorldWar.Abstractions.Exceptions;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

public class Storage<T> : IStorage<T>
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

	public void Set(Guid key, T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException(nameof(item));
		}

		ItemsStorage.AddOrUpdate(key, item, (_, _) => item);
	}

	public IEnumerable<T> Get()
	{
		return ItemsStorage.Values.Select(x => x);
	}

	public void Remove(Guid key)
	{
		ItemsStorage.TryRemove(key, out _);
	}

	public IEnumerable<T> GetByFilter(Func<T, bool> predicate)
	{
		return ItemsStorage.Values.Where(predicate);
	}
}