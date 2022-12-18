using System.Diagnostics;
using System.Numerics;
using Microsoft.Extensions.Logging;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

internal class MovableService : IMovableService
{
	private readonly ITaskDelay _taskDelay;
	private readonly ILogger<MovableService> _logger;

	public MovableService(ITaskDelay taskDelay, ILogger<MovableService> logger)
	{
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task StartMove(Unit unit, float[][] route, CancellationToken cancellationToken)
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();
		var index = 0;

		if (!route.Any())
		{
			return;
		}

		await Rotate(unit, route[index][0], route[index][1], cancellationToken);

		// get the travel time of the segment of the path
		var lastTime = TimeSpan.FromSeconds(Vector2.Distance(unit.Location.StartPos, new Vector2(route[index][1], route[index][0])) / unit.Speed);

		while (!cancellationToken.IsCancellationRequested)
		{
			// to avoid a miss, stop or turn
			var remainingTime = stopWatch.Elapsed - lastTime;

			// the remainingTime is greater than zero, then the endpoint was skipped
			_logger.LogDebug("The unit {id} will reach the intermediate point in {remainingTime} seconds", unit.Id, remainingTime.TotalSeconds);
			if (remainingTime.TotalMilliseconds < 0)
			{
				if (unit.UnitType == UnitTypes.Car)
				{
					//ToDo Add a state change
					//BUG Changing the state does not work correctly
				}

				unit.Move(lastTime + remainingTime, route[index][1], route[index][0]);
				await _taskDelay.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
			}
			else
			{
				unit.Move(lastTime, route[index][1], route[index][0]);
				unit.SaveCurrentLocation();

				index++;
				if (route.Length <= index)
				{
					break;
				}
				stopWatch.Restart();

				await unit.RotateUnit(route[index][1], route[index][0], route[index - 1][1], route[index - 1][0]);
				lastTime = TimeSpan.FromSeconds(Vector2.Distance(unit.Location.StartPos, new Vector2(route[index][1], route[index][0])) / unit.Speed);
			}
		}
		unit.SaveCurrentLocation();
	}

	public async Task StartMove(Unit unit, Unit targetUnit, CancellationToken cancellationToken,
		float? distance = null)
	{
		var startDateTime = DateTime.Now;
		while (!cancellationToken.IsCancellationRequested)
		{
			await unit.RotateUnit(targetUnit.Longitude, targetUnit.Latitude);
			await _taskDelay.Delay(TimeSpan.FromMilliseconds(300), cancellationToken).ConfigureAwait(false);

			unit.Move(DateTime.Now - startDateTime, targetUnit.Longitude, targetUnit.Latitude);

			if (unit.IsWithinReach(targetUnit.Longitude, targetUnit.Latitude, distance))
			{
				break;
			}
		}

		unit.SaveCurrentLocation();
	}

	public async Task Rotate(Unit unit, float latitude, float longitude, CancellationToken cancellationToken)
	{
		await unit.RotateUnit(longitude, latitude);
	}
}