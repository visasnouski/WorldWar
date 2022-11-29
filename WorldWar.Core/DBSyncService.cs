using Microsoft.Extensions.Hosting;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.Repository.interfaces;

namespace WorldWar.Core;

public class DbSyncService : BackgroundService
{
	private readonly IDbRepository _dbRepository;
	private readonly IStorage<Unit> _unitsStorage;
	private readonly ITaskDelay _taskDelay;

	public DbSyncService(IDbRepository dbRepository, ICacheFactory cacheFactory, ITaskDelay taskDelay)
	{
		_dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
		_unitsStorage = cacheFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(cacheFactory));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var dbUnits = _dbRepository.Units;
		_unitsStorage.SetItem(dbUnits);

		while (!stoppingToken.IsCancellationRequested)
		{
			await _taskDelay.Delay(TimeSpan.FromMinutes(1), CancellationToken.None).ConfigureAwait(true);
			var mapUnits = _unitsStorage.GetItems();
			await _dbRepository.SetUnits(mapUnits, stoppingToken).ConfigureAwait(true);
		}
	}
}
