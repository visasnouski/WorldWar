using System.Collections.Concurrent;
using System.Diagnostics;
using ConcurrentCollections;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;
using WorldWar.Interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Internal;

public class WorldWarMapService : IWorldWarMapService
{
	private readonly IMapStorage _mapStorage;
	private readonly IAuthUser _authUser;
	private readonly ITaskDelay _taskDelay;
	private readonly IYandexJsClientAdapter _yandexJsClientAdapter;
	private readonly ILogger<WorldWarMapService> _logger;

	public WorldWarMapService(IMapStorage mapStorage, IAuthUser authUser, ITaskDelay taskDelay, IYandexJsClientAdapter yandexJsClientAdapter, ILogger<WorldWarMapService> logger)
	{
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_yandexJsClientAdapter = yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public Task RunUnitsAutoRefreshAsync(bool viewAllUnits = false)
	{
		_ = Task.Run(async () =>
		{
			var authUser = await _authUser.GetIdentity().ConfigureAwait(true);
			var mapGuids = new ConcurrentHashSet<(Guid, UnitTypes)>();
			Stopwatch sw = new Stopwatch();

			try
			{
				while (true)
				{
					var visibleGuids = new ConcurrentHashSet<(Guid, UnitTypes)>();

					var task = viewAllUnits
						? _mapStorage.GetUnits()
						: _mapStorage.GetVisibleUnits(authUser.GuidId);

					var units = await task.ConfigureAwait(true);
					var toUpdateList = new ConcurrentBag<Unit>();
					sw.Reset();
					sw.Start();
					await Parallel.ForEachAsync(units, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, async (unit, token) =>
					{
						token.ThrowIfCancellationRequested();

						if (!mapGuids.Contains((unit.Id, unit.UnitType)))
						{
							await _yandexJsClientAdapter.AddUnit(unit).ConfigureAwait(true);
							mapGuids.Add((unit.Id, unit.UnitType));
						}

						visibleGuids.Add((unit.Id, unit.UnitType));
						toUpdateList.Add(unit);
					}).ConfigureAwait(true);
					sw.Stop();
					_logger.LogInformation("time: {ElapsetTime}", sw.ElapsedMilliseconds);

					if (toUpdateList.Any())
					{
						await _yandexJsClientAdapter.UpdateUnits(toUpdateList.ToArray()).ConfigureAwait(true);
					}

					var removableGuids = mapGuids.Except(visibleGuids).ToArray();
					if (removableGuids.Any())
					{
						await _yandexJsClientAdapter.RemoveGeoObjects(removableGuids.Select(x => x.Item1).ToArray()).ConfigureAwait(true);

						foreach (var removableGuid in removableGuids)
						{
							mapGuids.TryRemove(removableGuid);
						}
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