using System.Diagnostics;
using System.Numerics;
using Microsoft.Extensions.Logging;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

internal class MovableService : IMovableService
{
	private readonly IStorage<Unit> _unitsStorage;
	private readonly ITaskDelay _taskDelay;
	private readonly ILogger<MovableService> _logger;

	public MovableService(IStorageFactory storageFactory, ITaskDelay taskDelay, ILogger<MovableService> logger)
	{
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task StartMove(Guid unitId, float[][] route, CancellationToken cancellationToken)
	{
		if (!_unitsStorage.TryGetValue(unitId, out var user))
		{
			return;
		}

		var stopWatch = new Stopwatch();
		stopWatch.Start();
		var index = 0;

		if (!route.Any())
		{
			return;
		}

		// get the travel time of the segment of the path
		var lastTime = TimeSpan.FromSeconds(Vector2.Distance(user!.Location.StartPos, new Vector2(route[index][1], route[index][0])) / user.Speed);

		while (!cancellationToken.IsCancellationRequested)
		{
			// to avoid a miss, stop or turn
			var remainingTime = stopWatch.Elapsed - lastTime;

			// the remainingTime is greater than zero, then the endpoint was skipped
			_logger.LogDebug("The unit {id} will reach the intermediate point in {remainingTime} seconds", user.Id, remainingTime.TotalSeconds);
			if (remainingTime.TotalMilliseconds < 0)
			{
				if (user.UnitType == UnitTypes.Car)
				{
					//ToDo Add a state change
					//BUG Changing the state does not work correctly
				}

				user.Move(lastTime + remainingTime, route[index][1], route[index][0]);
				await _taskDelay.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
			}
			else
			{
				user.Move(lastTime, route[index][1], route[index][0]);
				user.SaveCurrentLocation();
				index++;
				if (route.Length <= index)
				{
					break;
				}
				stopWatch.Restart();
				await user.RotateUnit(route[index][1], route[index][0], route[index - 1][1], route[index - 1][0]);
				_unitsStorage.AddOrUpdate(user.Id, user);
				lastTime = TimeSpan.FromSeconds(Vector2.Distance(user.Location.StartPos, new Vector2(route[index][1], route[index][0])) / user.Speed);
			}
		}
		user.SaveCurrentLocation();
		_unitsStorage.AddOrUpdate(user.Id, user);
	}

	public async Task StartMove(Guid unitId, Guid targetGuid, CancellationToken cancellationToken,
		float? distance = null)
	{
		if (!_unitsStorage.TryGetValue(unitId, out var myUnit))
		{
			return;
		}

		var startDateTime = DateTime.Now;
		while (!cancellationToken.IsCancellationRequested)
		{
			if (!_unitsStorage.TryGetValue(targetGuid, out var targetUnit))
			{
				break;
			}

			await myUnit!.RotateUnit(targetUnit!.Longitude, targetUnit.Latitude);
			await _taskDelay.Delay(TimeSpan.FromMilliseconds(300), cancellationToken).ConfigureAwait(false);

			myUnit.Move(DateTime.Now - startDateTime, targetUnit.Longitude, targetUnit.Latitude);
			_unitsStorage.AddOrUpdate(myUnit.Id, myUnit);

			if (myUnit.IsWithinReach(targetUnit.Longitude, targetUnit.Latitude, distance))
			{
				break;
			}
		}

		myUnit!.SaveCurrentLocation();
	}

	public async Task Rotate(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken)
	{
		if (!_unitsStorage.TryGetValue(unitId, out var user))
		{
			return;
		}

		await user!.RotateUnit(longitude, latitude);
		_unitsStorage.AddOrUpdate(user.Id, user);
	}
}