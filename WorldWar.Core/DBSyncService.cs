using Microsoft.Extensions.Hosting;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Core.Interfaces;
using WorldWar.Repository.interfaces;

namespace WorldWar.Core;

public class DbSyncService : BackgroundService
{
	private readonly IDbRepository _dbRepository;
	private readonly IMapStorage _mapStorage;
	private readonly ITaskDelay _taskDelay;

	public DbSyncService(IDbRepository dbRepository, IMapStorage mapStorage, ITaskDelay taskDelay)
	{
		_dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var dbUnits = _dbRepository.Units;
		await _mapStorage.SetUnits(dbUnits).ConfigureAwait(true);

		while (!stoppingToken.IsCancellationRequested)
		{
			await _taskDelay.Delay(TimeSpan.FromMinutes(1), CancellationToken.None).ConfigureAwait(true);
			var mapUnits = await _mapStorage.GetUnits().ConfigureAwait(true);
			await _dbRepository.SetUnits(mapUnits).ConfigureAwait(true);
		}
	}
}
