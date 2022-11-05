using System.Diagnostics;
using System.Numerics;
using WorldWar.Abstractions;
using WorldWar.Abstractions.Models;
using WorldWar.Interfaces;

namespace WorldWar.Internal
{
	public class MovableService : IMovableService
	{
		private readonly IMapStorage _mapStorage;
		private readonly IAuthUser _authUser;
		private readonly IYandexJsClientTransmitter _yandexJsClientTransmitter;
		private readonly IYandexJsClientAdapter _yandexJsClientAdapter;
		private readonly ITaskDelay _taskDelay;
		private readonly ILogger<MovableService> _logger;

		public MovableService(IMapStorage mapStorage, IAuthUser authUser, IYandexJsClientTransmitter yandexJsClientTransmitter, IYandexJsClientAdapter yandexJsClientAdapter, ITaskDelay taskDelay, ILogger<MovableService> logger)
		{
			_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
			_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
			_yandexJsClientTransmitter = yandexJsClientTransmitter ?? throw new ArgumentNullException(nameof(yandexJsClientTransmitter));
			_yandexJsClientAdapter = yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
			_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task StartMove(float latitude, float longitude, CancellationToken cancellationToken,
			float? weaponDistance = null)
		{
			var identity = await _authUser.GetIdentity().ConfigureAwait(true);
			var user = await _mapStorage.GetUnit(identity.GuidId).ConfigureAwait(true);

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

					user.Move(lastTime + remainingTime, points[index][1], points[index][0]);
					await _taskDelay.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(true);
				}
				else
				{
					user.Move(lastTime, points[index][1], points[index][0]);
					user.SaveCurrentLocation();
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
			user.SaveCurrentLocation();
			await _mapStorage.SetUnit(user).ConfigureAwait(true);
		}

		public async Task StartMove(Guid targetGuid, CancellationToken cancellationToken,
			float? distance = null)
		{
			var identity = await _authUser.GetIdentity().ConfigureAwait(true);
			var myUnit = await _mapStorage.GetUnit(identity.GuidId).ConfigureAwait(true);
			var startDateTime = DateTime.Now;
			while (!cancellationToken.IsCancellationRequested)
			{
				var targetUnit = await _mapStorage.GetUnit(targetGuid).ConfigureAwait(true);

				await _taskDelay.Delay(TimeSpan.FromMilliseconds(300), cancellationToken).ConfigureAwait(true);

				myUnit.Move(DateTime.Now - startDateTime, targetUnit.CurrentLongitude, targetUnit.CurrentLatitude);
				await _mapStorage.SetUnit(myUnit).ConfigureAwait(true);

				if (myUnit.IsWithinReach(targetUnit.CurrentLongitude, targetUnit.CurrentLatitude, distance))
				{
					break;
				}
			}

			myUnit.SaveCurrentLocation();
		}

		public async Task Rotate(Guid id, float latitude, float longitude, CancellationToken cancellationToken)
		{
			await _yandexJsClientTransmitter.RotateUnit(id, latitude, longitude).ConfigureAwait(true);
		}
	}
}
