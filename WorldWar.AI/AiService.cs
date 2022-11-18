using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Core.Interfaces;

namespace WorldWar.AI;

internal class AiService : BackgroundService
{
	private readonly ILogger<AiService> _logger;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IMapStorage _mapStorage;
	private readonly ITaskDelay _taskDelay;

	public AiService(ILogger<AiService> logger, IServiceScopeFactory serviceScopeFactory, IMapStorage maspStorage, ITaskDelay taskDelay)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		_mapStorage = maspStorage ?? throw new ArgumentNullException(nameof(maspStorage));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
	}

	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var managementService = scope.ServiceProvider.GetRequiredService<IUnitManagementService>();
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				var units = await _mapStorage.GetUnits().ConfigureAwait(true);
				var mobs = units.Where(unit => unit.UnitType == UnitTypes.Mob
				                               && unit.Location.StartPos == unit.Location.CurrentPos
				                               && unit.Health > 0);
				var index = 0;
				await Parallel.ForEachAsync(mobs, cancellationToken, (unit, token) =>
				{
					token.ThrowIfCancellationRequested();

					var latitudeRnd = (float)RandomNumberGenerator.GetInt32(-99, 99) / 100;
					var longitudeRnd = (float)RandomNumberGenerator.GetInt32(-99, 99) / 100;
					var newLatitude = unit.CurrentLatitude + latitudeRnd;
					var newLongitude = unit.CurrentLongitude + longitudeRnd;

					_logger.LogInformation("{index} Unit {unitId} move to {latitude} {longitud}", index, unit.Id, newLatitude, newLongitude);
					unit.RotateUnit(newLongitude, newLatitude);
					managementService.MoveUnit(unit.Id, newLatitude, newLongitude).ConfigureAwait(true);
					index++;
					return ValueTask.CompletedTask;
				}).ConfigureAwait(true);

				await _taskDelay.Delay(TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(true);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}