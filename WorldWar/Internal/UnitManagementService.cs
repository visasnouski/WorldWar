using WorldWar.Abstractions;
using WorldWar.Core;
using WorldWar.Interfaces;

namespace WorldWar.Internal
{
	public class UnitManagementService : IUnitManagementService
	{
		private readonly ITasksStorage _tasksStorage;
		private readonly ICombatService _combatService;
		private readonly IMovableService _movableService;
		private readonly IInteractionObjectsService _interactionObjectsService;

		public UnitManagementService(ITasksStorage tasksStorage, ICombatService combatService, IMovableService movableService, IInteractionObjectsService interactionObjectsService)
		{
			_tasksStorage = tasksStorage ?? throw new ArgumentNullException(nameof(tasksStorage));
			_combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
			_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
			_interactionObjectsService = interactionObjectsService ?? throw new ArgumentNullException(nameof(interactionObjectsService));
		}

		public async Task MoveUnit(Guid unitId, float latitude, float longitude, bool useRoute = false)
		{
			await StopUnit(unitId).ConfigureAwait(true);

			Task Func(CancellationTokenSource cs) =>
				useRoute
					? _movableService.StartMoveAlongRoute(unitId, latitude, longitude, cs.Token)
					: _movableService.StartMoveToCoordinates(unitId, latitude, longitude, cs.Token);

			_tasksStorage.AddOrUpdate(unitId, GetTask(Func));
		}

		public async Task StopUnit(Guid unitId)
		{
			if (_tasksStorage.TryGetValue(unitId, out var task))
			{
				task!.Value.Item1.Cancel(false);

				try
				{
					await task.Value.Item2.ConfigureAwait(true);
					_tasksStorage.TryRemove(unitId);
				}
				catch (TaskCanceledException)
				{
				}
			}
		}

		public async Task Attack(Guid unitId, Guid enemyGuid)
		{
			await StopUnit(unitId).ConfigureAwait(true);
			_tasksStorage.AddOrUpdate(unitId, GetTask(cs => _combatService.AttackUnit(enemyGuid, cs.Token)));
		}

		public async Task GetInCar(Guid unitId, Guid itemGuid)
		{
			await StopUnit(unitId).ConfigureAwait(true);
			_tasksStorage.AddOrUpdate(unitId, GetTask(cs => _interactionObjectsService.GetIn(itemGuid, cs.Token)));
		}

		public async Task PickUp(Guid unitId, Guid itemGuid, bool isUnit)
		{
			await StopUnit(unitId).ConfigureAwait(true);
			_tasksStorage.AddOrUpdate(unitId, GetTask(cs => _interactionObjectsService.PickUp(itemGuid, isUnit, cs.Token)));
		}

		private static (CancellationTokenSource cs, Task task) GetTask(Func<CancellationTokenSource, Task> func)
		{
			var cs = new CancellationTokenSource();
			var attackTask = Task.Run(() => func(cs), cs.Token);
			return (cs, attackTask);
		}
	}
}
