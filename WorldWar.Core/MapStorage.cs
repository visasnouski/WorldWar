using System.Collections.Concurrent;
using System.Numerics;
using WorldWar.Abstractions.Exceptions;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Abstractions.Models.Units.Base;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

internal class MapStorage : IMapStorage
{
	private static readonly ConcurrentDictionary<Guid, Unit> UnitsStorage = new();
	private static readonly ConcurrentDictionary<Guid, Box> ItemsStorage = new();

	public async Task SetUnits(IEnumerable<Unit> units)
	{
		if (units == null)
		{
			throw new ArgumentNullException(nameof(units));
		}

		foreach (var unit in units)
		{
			await SetUnit(unit).ConfigureAwait(true);
		}
	}

	public Task SetItem(Box item)
	{
		if (item == null)
		{
			throw new ArgumentNullException(nameof(item));
		}

		ItemsStorage.AddOrUpdate(item.Id, item, (_, _) => item);
		return Task.CompletedTask;
	}

	public Task<IEnumerable<Unit>> GetUnits()
	{
		return Task.FromResult(UnitsStorage.Values.Select(x => x));
	}

	public Task<IEnumerable<Box>> GetItems()
	{
		return Task.FromResult(ItemsStorage.Values.Select(x => x));
	}

	public Task<IEnumerable<Unit>> GetVisibleUnits(Guid id)
	{
		if (!UnitsStorage.TryGetValue(id, out var user))
		{
			return Task.FromResult(Enumerable.Empty<Unit>());
		}

		return Task.FromResult(UnitsStorage.Values.Where(unit => CanSee(user, unit.Location)));
	}

	public Task<IEnumerable<Box>> GetVisibleItems(Guid id)
	{
		if (!UnitsStorage.TryGetValue(id, out var user))
		{
			return Task.FromResult(Enumerable.Empty<Box>());
		}

		return Task.FromResult(ItemsStorage.Values.Where(box => CanSee(user, box.Latitude, box.Longitude)));
	}

	public Task<Unit> GetUnit(Guid id)
	{
		if (UnitsStorage.TryGetValue(id, out var unit))
		{
			return Task.FromResult(unit);
		}

		throw new UnitNotFoundException("unit not found");
	}

	public Task<Box> GetItem(Guid id)
	{
		if (ItemsStorage.TryGetValue(id, out var item))
		{
			return Task.FromResult(item);
		}

		throw new ItemNotFoundException("item not found");
	}

	public Task SetUnit(Unit unit)
	{
		if (unit == null)
		{
			throw new ArgumentNullException(nameof(unit));
		}

		UnitsStorage.AddOrUpdate(unit.Id, unit, (_, _) => unit);
		return Task.CompletedTask;
	}

	public Task RemoveUnit(Unit unit)
	{
		if (unit == null)
		{
			throw new ArgumentNullException(nameof(unit));
		}

		UnitsStorage.TryRemove(unit.Id, out _);
		return Task.CompletedTask;
	}

	public Task RemoveItem(Box box)
	{
		if (box == null)
		{
			throw new ArgumentNullException(nameof(box));
		}

		ItemsStorage.TryRemove(box.Id, out _);
		return Task.CompletedTask;
	}

	private static bool CanSee(Unit user, Location location)
	{
		return location is null
			? throw new ArgumentNullException(nameof(location))
			: Vector2.Distance(user.Location.CurrentPos, location.CurrentPos) < user.ViewingDistance;
	}

	private static bool CanSee(Unit user, float latitude, float longitude)
	{
		return Vector2.Distance(user.Location.CurrentPos, new Vector2(longitude, latitude)) < user.ViewingDistance;
	}
}