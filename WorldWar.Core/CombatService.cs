using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Core;

public class CombatService : ICombatService
{
	private readonly IStorage<Unit> _unitsStorage;
	private readonly IMovableService _movableService;
	private readonly ITasksStorage _tasksStorage;
	private readonly ITaskDelay _taskDelay;
	private readonly IYandexJsClientNotifier _yandexJsClientNotifier;
	private readonly ILogger<CombatService> _logger;

	public CombatService(ICacheFactory cacheFactory, IMovableService movableService, ITasksStorage tasksStorage, ITaskDelay taskDelay, IYandexJsClientNotifier yandexJsClientNotifier, ILogger<CombatService> logger)
	{
		_unitsStorage = cacheFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(cacheFactory));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_tasksStorage = tasksStorage ?? throw new ArgumentNullException(nameof(tasksStorage));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_yandexJsClientNotifier = yandexJsClientNotifier ?? throw new ArgumentNullException(nameof(yandexJsClientNotifier));
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

			user.RotateUnit(enemy!.Longitude, enemy.Latitude);

			if (user.Weapon.Ammo <= 0)
			{
				await _yandexJsClientNotifier.PlaySound("sound", user.Weapon.ReloadSoundLocation).ConfigureAwait(false);
				await ReloadWithDelay(user.Weapon, _taskDelay, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				var damage = user.CalculateDamage(enemy);
				enemy.AddDamage(damage);

				await _yandexJsClientNotifier.ShootUnit(user.Id, enemy.Latitude, enemy.Longitude).ConfigureAwait(false);
				await _yandexJsClientNotifier.PlaySound("sound", user.Weapon.ShotSoundLocation).ConfigureAwait(false);

				var message = damage > 0 ? damage.ToString(NumberFormatInfo.CurrentInfo) : "missed!";

				await _yandexJsClientNotifier.SendMessage(enemy.Id, message).ConfigureAwait(false);
				await ShootWithDelay(user.Weapon, _taskDelay, cancellationToken).ConfigureAwait(false);
			}

			if (enemy.Health <= 0)
			{
				await KillUnit(enemy).ConfigureAwait(false);
				break;
			}

			_unitsStorage.AddOrUpdate(enemy.Id, enemy);

			if (user.Health <= 0)
			{
				await KillUnit(user).ConfigureAwait(false);
				break;
			}

			_unitsStorage.AddOrUpdate(user.Id, user);
		}
	}

	private async Task KillUnit(Unit unit)
	{
		if (_tasksStorage.TryGetValue(unit.Id, out var task))
		{
			task!.Value.Item1.Cancel();
		}
		_tasksStorage.TryRemove(unit.Id);

		await _yandexJsClientNotifier.KillUnit(unit.Id).ConfigureAwait(false);
	}

	private static async Task ShootWithDelay(Weapon weapon, ITaskDelay delay, CancellationToken cancellationToken)
	{
		var delayShot = (int)weapon.DelayShot.TotalMilliseconds;
		var rndDelay = RandomNumberGenerator.GetInt32(delayShot / 2, delayShot);
		await delay.Delay(TimeSpan.FromMilliseconds(rndDelay), cancellationToken).ConfigureAwait(false);
		weapon.Ammo -= 1;
	}

	private static async Task ReloadWithDelay(Weapon weapon, ITaskDelay delay, CancellationToken cancellationToken)
	{
		await delay.Delay(weapon.ReloadTime, cancellationToken).ConfigureAwait(false);
		weapon.Ammo = weapon.MagazineSize;
	}
}