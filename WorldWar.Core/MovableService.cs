using System.Diagnostics;
using System.Numerics;
using Microsoft.Extensions.Logging;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Core;

internal class MovableService : IMovableService
{
	private readonly IStorage<Unit> _unitsStorage;
	private readonly IYandexJsClientNotifier _yandexJsClientNotifier;
	private readonly IYandexJsClientAdapter _yandexJsClientAdapter;
	private readonly ITaskDelay _taskDelay;
	private readonly ILogger<MovableService> _logger;

	public MovableService(ICacheFactory cacheFactory, IYandexJsClientNotifier yandexJsClientNotifier, IYandexJsClientAdapter yandexJsClientAdapter, ITaskDelay taskDelay, ILogger<MovableService> logger)
	{
		_unitsStorage = cacheFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(cacheFactory));
		_yandexJsClientNotifier = yandexJsClientNotifier ?? throw new ArgumentNullException(nameof(yandexJsClientNotifier));
		_yandexJsClientAdapter = yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task StartMoveAlongRoute(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken,
		float? weaponDistance = null)
	{
		if (!_unitsStorage.TryGetValue(unitId, out var user))
		{
			return;
		}

		var routingMode = user!.UnitType == UnitTypes.Car ? "auto" : "pedestrian";

		var points = await _yandexJsClientAdapter.GetRoute(new[] { user.Latitude, user.Longitude },
			new[] { latitude, longitude }, routingMode).ConfigureAwait(true);

		var stopWatch = new Stopwatch();
		stopWatch.Start();
		var index = 0;

		if (!points.Any())
		{
			return;
		}

		// get the travel time of the segment of the path
		var lastTime = TimeSpan.FromSeconds(Vector2.Distance(user.Location.StartPos, new Vector2(points[index][1], points[index][0])) / user.Speed);

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
					//ToDo Acceleration
				}

				Move(user, lastTime + remainingTime, points[index][1], points[index][0]);
				await _taskDelay.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(true);
			}
			else
			{
				Move(user, lastTime, points[index][1], points[index][0]);
				user.SaveCurrentLocation();
				index++;
				if (points.Length <= index)
				{
					break;
				}
				stopWatch.Restart();
				user.RotateUnit(points[index][1], points[index][0], points[index - 1][1], points[index - 1][0]);
				_unitsStorage.Set(user.Id, user);
				lastTime = TimeSpan.FromSeconds(Vector2.Distance(user.Location.StartPos, new Vector2(points[index][1], points[index][0])) / user.Speed);
			}
		}
		user.SaveCurrentLocation();
		_unitsStorage.Set(user.Id, user);
	}

	public async Task StartMoveToCoordinates(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken,
		float? weaponDistance = null)
	{
		if (!_unitsStorage.TryGetValue(unitId, out var user))
		{
			return;
		}

		var stopWatch = new Stopwatch();
		stopWatch.Start();

		// get the travel time of the segment of the path
		var lastTime = TimeSpan.FromSeconds(Vector2.Distance(user!.Location.StartPos, new Vector2(longitude, latitude)) / user.Speed);

		while (!cancellationToken.IsCancellationRequested)
		{
			// to avoid a miss, stop or turn
			var remainingTime = stopWatch.Elapsed - lastTime;

			// the remainingTime is greater than zero, then the endpoint was skipped
			if (remainingTime.TotalMilliseconds < 0)
			{
				if (user.UnitType == UnitTypes.Car)
				{
					//ToDo Acceleration
				}

				Move(user, lastTime + remainingTime, longitude, latitude);
				await _taskDelay.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(true);
			}
			else
			{
				Move(user, lastTime, longitude, latitude);
				user.SaveCurrentLocation();

				stopWatch.Restart();
				user.RotateUnit(longitude, latitude, user.Longitude, user.Latitude);
				_unitsStorage.Set(user.Id, user);
			}
		}
		user.SaveCurrentLocation();
		_unitsStorage.Set(user.Id, user);
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

			myUnit!.RotateUnit(targetUnit!.Longitude, targetUnit.Latitude);
			await _taskDelay.Delay(TimeSpan.FromMilliseconds(300), cancellationToken).ConfigureAwait(true);

			Move(myUnit!, DateTime.Now - startDateTime, targetUnit.Longitude, targetUnit.Latitude);
			_unitsStorage.Set(myUnit!.Id, myUnit);

			if (myUnit.IsWithinReach(targetUnit.Longitude, targetUnit.Latitude, distance))
			{
				break;
			}
		}

		myUnit!.SaveCurrentLocation();
	}

	public async Task Rotate(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken)
	{
		await _yandexJsClientNotifier.RotateUnit(unitId, latitude, longitude).ConfigureAwait(true);
	}

	private static void Move(Unit unit, TimeSpan time, float endLongitude, float endLatitude, int acceleration = 1)
	{
		var endPos = new Vector2(endLongitude, endLatitude);
		var movVec = Vector2.Subtract(endPos, unit.Location.StartPos);
		var normMovVec = Vector2.Normalize(movVec);

		if (normMovVec.X is Single.NaN || normMovVec.Y is Single.NaN)
		{
			return;
		}

		var deltaVec = normMovVec * Convert.ToInt64(time.TotalSeconds) * unit.Speed * acceleration;
		unit.Location.ChangeLocation(Vector2.Add(unit.Location.StartPos, deltaVec));
	}
}