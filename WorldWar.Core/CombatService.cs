using Microsoft.Extensions.Logging;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

public class CombatService : ICombatService
{
	private readonly IMovableService _movableService;
	private readonly ITasksStorage _tasksStorage;
	private readonly ITaskDelay _taskDelay;
	private readonly ILogger<CombatService> _logger;

	public CombatService(IMovableService movableService, ITasksStorage tasksStorage, ITaskDelay taskDelay, ILogger<CombatService> logger)
	{
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_tasksStorage = tasksStorage ?? throw new ArgumentNullException(nameof(tasksStorage));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task AttackUnit(Unit user, Unit enemy, CancellationToken cancellationToken)
	{
		if (enemy.Health <= 0)
		{
			_logger.LogInformation("The enemy {Id} has already been defeated.", enemy.Id);
			return;
		}

		if (user.Health <= 0)
		{
			_logger.LogInformation("The user {Id} has already been defeated.", enemy.Id);
			return;
		}

		if (user.Weapon.WeaponType == WeaponTypes.Handguns
			&& !user.IsWithinReach(enemy.Longitude, enemy.Latitude, user.Weapon.Distance))
		{
			_logger.LogInformation("The user {Id} will move due to the long distance {Distance} to the enemy {Id}.", user.Id, user.Weapon.Distance, enemy.Id);
			await _movableService.StartMove(user, enemy, cancellationToken, user.Weapon.Distance);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		await _movableService.Rotate(user, enemy.Latitude, enemy.Longitude, cancellationToken);

		while (!cancellationToken.IsCancellationRequested)
		{
			await user.Shoot(enemy, _taskDelay, cancellationToken);

			if (enemy.Health <= 0)
			{
				_logger.LogInformation("The enemy {Id} is defeated.", enemy.Id);
				RemoveTasksForUnit(enemy);
				break;
			}

			if (user.Health <= 0)
			{
				_logger.LogInformation("The user {Id} is defeated.", enemy.Id);
				RemoveTasksForUnit(user);
				break;
			}
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