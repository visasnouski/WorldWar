using Microsoft.Extensions.Hosting;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Repository.interfaces;

namespace WorldWar.Core
{
	public class DBSyncService: BackgroundService
	{
		private readonly IDbRepository _dbRepository;
		private readonly IMapStorage _mapStorage;
		private readonly ITaskDelay _taskDelay;

		public DBSyncService(IDbRepository dbRepository, IMapStorage mapStorage, ITaskDelay taskDelay)
		{
			_dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
			_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
			_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		}
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_ = Task.Run(async () =>
			{
				while (true)
				{
					var dbUnits = _dbRepository.Units;
					await _mapStorage.SetUnits(dbUnits).ConfigureAwait(true);
					var mapUnits = await _mapStorage.GetUnits().ConfigureAwait(true);
					await _taskDelay.Delay(TimeSpan.FromMinutes(1), CancellationToken.None).ConfigureAwait(true);
					await _dbRepository.SetUnits(mapUnits).ConfigureAwait(true);
				}
			}, CancellationToken.None);

			return Task.CompletedTask;
		}
	}
}