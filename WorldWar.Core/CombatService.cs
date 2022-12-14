using Microsoft.Extensions.Logging;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

public class CombatService : ICombatService
{
	private readonly IStorage<Unit> _unitsStorage;
	private readonly IMovableService _movableService;
	private readonly ITasksStorage _tasksStorage;
	private readonly ITaskDelay _taskDelay;
	private readonly ILogger<CombatService> _logger;

	public CombatService(IStorageFactory storageFactory, IMovableService movableService, ITasksStorage tasksStorage, ITaskDelay taskDelay, ILogger<CombatService> logger)
	{
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_tasksStorage = tasksStorage ?? throw new ArgumentNullException(nameof(tasksStorage));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task AttackUnit(Guid userGuid, Guid enemyGuid, CancellationToken cancellationToken)
	{
		if (!_unitsStorage.TryGetValue(userGuid, out var user))
		{
			_logger.LogWarning("The user {guid} not found.", userGuid);
			return;
		}

		if (!_unitsStorage.TryGetValue(enemyGuid, out var enemy))
		{
			_logger.LogWarning("The enemy {guid} not found.", enemyGuid);
			return;
		}

		if (user!.Weapon.WeaponType == WeaponTypes.Handguns
			&& !user.IsWithinReach(enemy!.Longitude, enemy.Latitude, user.Weapon.Distance))
		{
			await _movableService.StartMove(user.Id, enemy.Id, cancellationToken, user.Weapon.Distance).ConfigureAwait(false);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		await _movableService.Rotate(user.Id, enemy!.Latitude, enemy.Longitude, cancellationToken).ConfigureAwait(false);

		while (!cancellationToken.IsCancellationRequested)
		{
			if (!_unitsStorage.TryGetValue(enemyGuid, out enemy))
			{
				return;
			}

			await user.Shoot(enemy!, _taskDelay, cancellationToken);

			if (enemy!.Health <= 0)
			{
				RemoveTasksForUnit(enemy);
				break;
			}

			_unitsStorage.AddOrUpdate(enemy.Id, enemy);

			if (user.Health <= 0)
			{
				RemoveTasksForUnit(user);
				break;
			}

			_unitsStorage.AddOrUpdate(user.Id, user);
		}
	}

	private void RemoveTasksForUnit(Unit unit)
	{
		if (_tasksStorage.TryGetValue(unit.Id, out var task))
		{
			task!.Value.Item1.Cancel();
		}
		_tasksStorage.TryRemove(unit.Id);
	}
}