using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;

namespace WorldWar.AI;

// Supports only movement
internal class AiService : BackgroundService
{
	private readonly ILogger<AiService> _logger;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IStorage<Unit> _unitsStorage;
	private readonly ITaskDelay _taskDelay;

	public AiService(ILogger<AiService> logger, IServiceScopeFactory serviceScopeFactory, IStorageFactory storageFactory, ITaskDelay taskDelay)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
	}

	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var managementService = scope.ServiceProvider.GetRequiredService<IUnitManagementService>();
		while (!cancellationToken.IsCancellationRequested)
		{
			var units = _unitsStorage.Get();
			var mobs = units.Where(unit => unit.UnitType == UnitTypes.Mob
										   && unit.Location.StartPos == unit.Location.CurrentPos
										   && unit.Health > 0);

			foreach (var unit in mobs)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var latitudeRnd = (float)RandomNumberGenerator.GetInt32(-99, 99) / 100;
				var longitudeRnd = (float)RandomNumberGenerator.GetInt32(-99, 99) / 100;
				var newLatitude = unit.Latitude + latitudeRnd;
				var newLongitude = unit.Longitude + longitudeRnd;

				await unit.RotateUnit(newLongitude, newLatitude);

				_logger.LogInformation("Move unit {id} to [{latitude}, {longitude}]", unit.Id, newLatitude, newLongitude);
				await managementService.MoveUnit(unit, newLatitude, newLongitude).ConfigureAwait(true);
			}

			await _taskDelay.Delay(TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(true);
		}
	}
}