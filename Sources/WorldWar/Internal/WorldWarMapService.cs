using System.Collections.Concurrent;
using System.Numerics;
using WorldWar.Abstractions.Exceptions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.Interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Internal;

internal class WorldWarMapService : IWorldWarMapService
{
	private readonly IStorage<Unit> _unitsStorage;
	private readonly IStorage<Box> _boxStorage;
	private readonly IAuthUser _authUser;
	private readonly ITaskDelay _taskDelay;
	private readonly IYandexJsClientAdapter _yandexJsClientAdapter;
	private readonly ILogger<WorldWarMapService> _logger;

	public WorldWarMapService(IStorageFactory storageFactory, IAuthUser authUser, ITaskDelay taskDelay, IYandexJsClientAdapter yandexJsClientAdapter, ILogger<WorldWarMapService> logger)
	{
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_boxStorage = storageFactory.Create<Box>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_yandexJsClientAdapter = yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public Task RunUnitsAutoRefreshAsync(bool viewAllUnits = false)
	{
		_ = Task.Run(async () =>
		{
			var authUser = await _authUser.GetIdentity();
			var mapGuids = new HashSet<(Guid, UnitTypes)>();

			while (true)
			{
				var visibleGuids = new HashSet<(Guid, UnitTypes)>();

				var units = GetUnits(viewAllUnits, authUser.GuidId);

				var toUpdateList = new ConcurrentBag<Unit>();

				foreach (var unit in units)
				{
					if (!mapGuids.Contains((unit.Id, unit.UnitType)))
					{
						await _yandexJsClientAdapter.AddUnit(unit);
						mapGuids.Add((unit.Id, unit.UnitType));
					}

					visibleGuids.Add((unit.Id, unit.UnitType));
					toUpdateList.Add(unit);
				}

				if (toUpdateList.Any())
				{
					await _yandexJsClientAdapter.UpdateUnits(toUpdateList.ToArray());
				}

				var removableGuids = mapGuids.Except(visibleGuids).ToArray();
				if (removableGuids.Any())
				{
					await _yandexJsClientAdapter.RemoveGeoObjects(removableGuids.Select(x => x.Item1).ToArray());

					foreach (var removableGuid in removableGuids)
					{
						mapGuids.Remove(removableGuid);
					}
				}
				await _taskDelay.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
			}

		}, CancellationToken.None);

		return Task.CompletedTask;
	}

	public Task RunItemsAutoRefresh(bool viewAllItems = false)
	{
		_ = Task.Run(async () =>
		{
			var authUser = await _authUser.GetIdentity();
			var mapGuids = new HashSet<Guid>();

			while (true)
			{
				var visibleGuids = new HashSet<Guid>();

				var items = GetItems(viewAllItems, authUser.GuidId);

				foreach (var item in items)
				{
					if (!mapGuids.Contains(item.Id))
					{
						await _yandexJsClientAdapter.AddBox(item);
						mapGuids.Add(item.Id);
					}

					visibleGuids.Add(item.Id);
				}

				var guidsToRemove = mapGuids.Except(visibleGuids).ToArray();
				if (guidsToRemove.Any())
				{
					await _yandexJsClientAdapter.RemoveGeoObjects(guidsToRemove);
					mapGuids.ExceptWith(guidsToRemove);
				}
				await _taskDelay.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
			}
		}, CancellationToken.None);

		return Task.CompletedTask;
	}

	private IEnumerable<Box> GetItems(bool viewAllItems, Guid id)
	{
		try
		{
			if (!_unitsStorage.TryGetValue(id, out var user))
			{
				return Enumerable.Empty<Box>();
			}
			var items = viewAllItems
				? _boxStorage.Get()
				: _boxStorage.GetByFilter(item =>
					CanSee(user!.Latitude, user.Longitude, item.Latitude, item.Longitude, user.ViewingDistance));
			return items;
		}
		catch (ItemNotFoundException)
		{
			_logger.LogWarning("The User {id} not found.", id);
			return Enumerable.Empty<Box>();
		}
	}

	private IEnumerable<Unit> GetUnits(bool viewAllUnits, Guid id)
	{
		try
		{
			if (!_unitsStorage.TryGetValue(id, out var user))
			{
				return Enumerable.Empty<Unit>();
			}

			var units = viewAllUnits
			? _unitsStorage.Get()
			: _unitsStorage.GetByFilter(item =>
				CanSee(user!.Latitude, user.Longitude, item.Latitude, item.Longitude, user.ViewingDistance));
			return units;
		}
		catch (ItemNotFoundException)
		{
			_logger.LogWarning("The User {id} not found.", id);
			return Enumerable.Empty<Unit>();
		}
	}

	private static bool CanSee(float centerLatitude, float centerLongitude, float latitude, float longitude, float viewingDistance)
	{
		return Vector2.Distance(new Vector2(centerLongitude, centerLatitude), new Vector2(longitude, latitude)) < viewingDistance;
	}
}