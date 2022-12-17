using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.Repository.interfaces;

namespace WorldWar.Core.BackgroundServices;

public class DbSyncService : BackgroundService
{
	private readonly IDbRepository _dbRepository;
	private readonly IStorage<Unit> _unitsStorage;
	private readonly ITaskDelay _taskDelay;
	private readonly ILogger<DbSyncService> _logger;

	public DbSyncService(IDbRepository dbRepository, IStorageFactory storageFactory, ITaskDelay taskDelay, ILogger<DbSyncService> logger)
	{
		_dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var dbUnits = _dbRepository.Units;

		foreach (var unit in dbUnits)
		{
			_unitsStorage.AddOrUpdate(unit.Id, unit);
		}

		while (!stoppingToken.IsCancellationRequested)
		{
			await _taskDelay.Delay(TimeSpan.FromMinutes(1), CancellationToken.None).ConfigureAwait(false);
			var mapUnits = _unitsStorage.Get();
			await _dbRepository.SetUnits(mapUnits, stoppingToken).ConfigureAwait(false);
		}
	}
}
