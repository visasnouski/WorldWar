using System.Diagnostics;
using System.Numerics;
using WorldWar.Abstractions;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Internal;

public class MovableService : IMovableService
{
	private readonly IMapStorage _mapStorage;
	private readonly IYandexJsClientTransmitter _yandexJsClientTransmitter;
	private readonly IYandexJsClientAdapter _yandexJsClientAdapter;
	private readonly ITaskDelay _taskDelay;
	private readonly ILogger<MovableService> _logger;

	public MovableService(IMapStorage mapStorage, IYandexJsClientTransmitter yandexJsClientTransmitter, IYandexJsClientAdapter yandexJsClientAdapter, ITaskDelay taskDelay, ILogger<MovableService> logger)
	{
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_yandexJsClientTransmitter = yandexJsClientTransmitter ?? throw new ArgumentNullException(nameof(yandexJsClientTransmitter));
		_yandexJsClientAdapter = yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task StartMoveAlongRoute(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken,
		float? weaponDistance = null)
	{
		var user = await _mapStorage.GetUnit(unitId).ConfigureAwait(true);

		var routingMode = user.UnitType == UnitTypes.Car ? "auto" : "pedestrian";

		var points = await _yandexJsClientAdapter.GetRoute(new[] { user.CurrentLatitude, user.CurrentLongitude },
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
				SaveCurrentLocation(user);
				index++;
				if (points.Length <= index)
				{
					break;
				}
				stopWatch.Restart();
				user.RotateUnit(points[index][1], points[index][0], points[index - 1][1], points[index - 1][0]);
				await _mapStorage.SetUnit(user).ConfigureAwait(true);
				lastTime = TimeSpan.FromSeconds(Vector2.Distance(user.Location.StartPos, new Vector2(points[index][1], points[index][0])) / user.Speed);
			}
		}
		SaveCurrentLocation(user);
		await _mapStorage.SetUnit(user).ConfigureAwait(true);
	}

	public async Task StartMoveToCoordinates(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken,
		float? weaponDistance = null)
	{
		var user = await _mapStorage.GetUnit(unitId).ConfigureAwait(true);

		var stopWatch = new Stopwatch();
		stopWatch.Start();

		// get the travel time of the segment of the path
		var lastTime = TimeSpan.FromSeconds(Vector2.Distance(user.Location.StartPos, new Vector2(longitude, latitude)) / user.Speed);

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
				SaveCurrentLocation(user);

				stopWatch.Restart();
				user.RotateUnit(longitude, latitude, user.CurrentLongitude, user.CurrentLatitude);
				await _mapStorage.SetUnit(user).ConfigureAwait(true);
			}
		}
		SaveCurrentLocation(user);
		await _mapStorage.SetUnit(user).ConfigureAwait(true);
	}

	public async Task StartMove(Guid unitId, Guid targetGuid, CancellationToken cancellationToken,
		float? distance = null)
	{
		var myUnit = await _mapStorage.GetUnit(unitId).ConfigureAwait(true);
		var startDateTime = DateTime.Now;
		while (!cancellationToken.IsCancellationRequested)
		{
			var targetUnit = await _mapStorage.GetUnit(targetGuid).ConfigureAwait(true);

			await _taskDelay.Delay(TimeSpan.FromMilliseconds(300), cancellationToken).ConfigureAwait(true);

			Move(myUnit, DateTime.Now - startDateTime, targetUnit.CurrentLongitude, targetUnit.CurrentLatitude);
			await _mapStorage.SetUnit(myUnit).ConfigureAwait(true);

			if (myUnit.IsWithinReach(targetUnit.CurrentLongitude, targetUnit.CurrentLatitude, distance))
			{
				break;
			}
		}

		SaveCurrentLocation(myUnit);
	}

	public async Task Rotate(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken)
	{
		await _yandexJsClientTransmitter.RotateUnit(unitId, latitude, longitude).ConfigureAwait(true);
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

	private static void SaveCurrentLocation(Unit unit)
	{
		unit.Location.SaveCurrentLocation();
	}
}