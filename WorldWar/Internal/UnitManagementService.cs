﻿using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;
using WorldWar.Interfaces;

namespace WorldWar.Internal;

internal class UnitManagementService : IUnitManagementService
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

	public async Task MoveUnit(Unit unit, float[][] route)
	{
		await StopUnit(unit.Id).ConfigureAwait(true);
		_tasksStorage.AddOrUpdate(unit.Id, GetTask(cs => _movableService.StartMove(unit, route, cs.Token)));
	}

	public async Task MoveUnit(Unit unit, float latitude, float longitude)
	{
		float[][] route = { new[] { latitude, longitude } };
		await MoveUnit(unit, route);
	}

	public async Task StopUnit(Guid unitId)
	{
		if (_tasksStorage.TryGetValue(unitId, out var task))
		{
			task!.Value.Item1.Cancel(false);

			try
			{
				await task.Value.Item2.ConfigureAwait(true);
			}
			catch (TaskCanceledException)
			{
			}

			_tasksStorage.TryRemove(unitId);
		}
	}

	public async Task Attack(Unit unit, Unit enemy)
	{
		await StopUnit(unit.Id).ConfigureAwait(true);
		_tasksStorage.AddOrUpdate(unit.Id, GetTask(cs => _combatService.AttackUnit(unit, enemy, cs.Token)));
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