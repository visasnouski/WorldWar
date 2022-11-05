using WorldWar.Abstractions;
using WorldWar.Abstractions.Models;
using WorldWar.Interfaces;

namespace WorldWar.Internal;

public class WorldWarMapService : IWorldWarMapService
{
	private readonly IMapStorage _mapStorage;
	private readonly IAuthUser _authUser;
	private readonly ITaskDelay _taskDelay;
	private readonly IDbRepository _dbRepository;
	private readonly IYandexJsClientAdapter _yandexJsClientAdapter;

	public WorldWarMapService(IMapStorage mapStorage, IAuthUser authUser, IDbRepository dbRepository, ITaskDelay taskDelay, IYandexJsClientAdapter yandexJsClientAdapter)
	{
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
		_dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_yandexJsClientAdapter = yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
	}

	public async Task RunAutoDbSync()
	{
		var dbUnits = _dbRepository.Units;
		await _mapStorage.SetUnits(dbUnits).ConfigureAwait(true);

		_ = Task.Run(async () =>
		{
			while (true)
			{
				var mapUnits = await _mapStorage.GetUnits().ConfigureAwait(true);
				await _dbRepository.SetUnits(mapUnits).ConfigureAwait(true);
				await _taskDelay.Delay(TimeSpan.FromMinutes(1), CancellationToken.None).ConfigureAwait(true);
			}
		}, CancellationToken.None);
	}

	public Task RunUnitsAutoRefresh(bool viewAllUnits = false)
	{
		_ = Task.Run(async () =>
		{
			var authUser = await _authUser.GetIdentity().ConfigureAwait(true);
			var mapGuids = new HashSet<(Guid, UnitTypes)>();
			try
			{


				while (true)
				{
					var visibleGuids = new HashSet<(Guid, UnitTypes)>();

					var task = viewAllUnits
							? _mapStorage.GetUnits()
							: _mapStorage.GetVisibleUnits(authUser.GuidId);

					var units = await task.ConfigureAwait(true);

					foreach (var unit in units)
					{
						if (!mapGuids.Contains((unit.Id, unit.UnitType)))
						{
							await _yandexJsClientAdapter.AddUnit(unit).ConfigureAwait(true);
							mapGuids.Add((unit.Id, unit.UnitType));
						}

						visibleGuids.Add((unit.Id, unit.UnitType));

						await _yandexJsClientAdapter.UpdateUnit(unit).ConfigureAwait(true);
					}

					var guidsToRemove = mapGuids.Except(visibleGuids).ToArray();
					if (guidsToRemove.Any())
					{
						await _yandexJsClientAdapter.RemoveGeoObjects(guidsToRemove.Select(x=>x.Item1).ToArray()).ConfigureAwait(true);
						mapGuids.ExceptWith(guidsToRemove);
					}
					await _taskDelay.Delay(TimeSpan.FromSeconds(1), CancellationToken.None).ConfigureAwait(true);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}, CancellationToken.None);

		return Task.CompletedTask;
	}

	public Task RunItemsAutoRefresh(bool viewAllItems = false)
	{
		_ = Task.Run(async () =>
		{
			var authUser = await _authUser.GetIdentity().ConfigureAwait(true);
			var mapGuids = new HashSet<Guid>();

			while (true)
			{
				var visibleGuids = new HashSet<Guid>();

				var task = viewAllItems
					? _mapStorage.GetItems()
					: _mapStorage.GetVisibleItems(authUser.GuidId);

				var items = await task.ConfigureAwait(true);

				foreach (var item in items)
				{
					if (!mapGuids.Contains(item.Id))
					{
						await _yandexJsClientAdapter.AddBox(item).ConfigureAwait(true);
						mapGuids.Add(item.Id);
					}

					visibleGuids.Add(item.Id);
				}

				var guidsToRemove = mapGuids.Except(visibleGuids).ToArray();
				if (guidsToRemove.Any())
				{
					await _yandexJsClientAdapter.RemoveGeoObjects(guidsToRemove).ConfigureAwait(true);
					mapGuids.ExceptWith(guidsToRemove);
				}
				await _taskDelay.Delay(TimeSpan.FromSeconds(1), CancellationToken.None).ConfigureAwait(true);
			}
		}, CancellationToken.None);

		return Task.CompletedTask;
	}
}