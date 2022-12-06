using System.Globalization;
using System.Security.Cryptography;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Core;

internal class CombatService : ICombatService
{
	private readonly IStorage<Unit> _unitsStorage;
	private readonly IMovableService _movableService;
	private readonly ITasksStorage _tasksStorage;
	private readonly ITaskDelay _taskDelay;
	private readonly IAuthUser _authUser;
	private readonly IYandexJsClientNotifier _yandexJsClientNotifier;

	public CombatService(ICacheFactory cacheFactory, IMovableService movableService, ITasksStorage tasksStorage, ITaskDelay taskDelay, IYandexJsClientNotifier yandexJsClientNotifier, IAuthUser authUser)
	{
		_unitsStorage = cacheFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(cacheFactory));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_tasksStorage = tasksStorage ?? throw new ArgumentNullException(nameof(tasksStorage));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_yandexJsClientNotifier = yandexJsClientNotifier ?? throw new ArgumentNullException(nameof(yandexJsClientNotifier));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
	}

	public async Task AttackUnit(Guid enemyGuid, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		if (!_unitsStorage.TryGetValue(identity.GuidId, out var user)
			|| !_unitsStorage.TryGetValue(enemyGuid, out var enemy))
		{
			return;
		}

		if (user!.Weapon.WeaponType == WeaponTypes.Handguns && !user.IsWithinReach(enemy!.Longitude, enemy.Latitude, user.Weapon.Distance))
		{
			await _movableService.StartMove(user.Id, enemy.Id, cancellationToken, user.Weapon.Distance).ConfigureAwait(true);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		await _movableService.Rotate(user.Id, enemy!.Latitude, enemy.Longitude, cancellationToken).ConfigureAwait(true);

		while (!cancellationToken.IsCancellationRequested)
		{
			if (!_unitsStorage.TryGetValue(enemyGuid, out enemy))
			{
				return;
			}

			user.RotateUnit(enemy!.Longitude, enemy.Latitude);

			if (user.Weapon.Ammo <= 0)
			{
				await _yandexJsClientNotifier.PlaySound("sound", user.Weapon.ReloadSoundLocation).ConfigureAwait(true);
				await ReloadWithDelay(user.Weapon, _taskDelay, cancellationToken).ConfigureAwait(true);
			}
			else
			{
				var damage = user.CalculateDamage(enemy);
				enemy.AddDamage(damage);

				await _yandexJsClientNotifier.ShootUnit(user.Id, enemy.Latitude, enemy.Longitude).ConfigureAwait(true);
				await _yandexJsClientNotifier.PlaySound("sound", user.Weapon.ShotSoundLocation).ConfigureAwait(true);

				var message = damage > 0 ? damage.ToString(NumberFormatInfo.CurrentInfo) : "missed!";

				await _yandexJsClientNotifier.SendMessage(enemy.Id, message).ConfigureAwait(true);
				await ShootWithDelay(user.Weapon, _taskDelay, cancellationToken).ConfigureAwait(true);
			}

			if (enemy.Health <= 0)
			{
				await KillUnit(enemy).ConfigureAwait(true);
				break;
			}

			_unitsStorage.Set(enemy.Id, enemy);

			if (user.Health <= 0)
			{
				await KillUnit(user).ConfigureAwait(true);
				break;
			}

			_unitsStorage.Set(user.Id, user);
		}
	}

	private async Task KillUnit(Unit unit)
	{
		if (_tasksStorage.TryGetValue(unit.Id, out var task))
		{
			task!.Value.Item1.Cancel();
		}
		_tasksStorage.TryRemove(unit.Id);

		await _yandexJsClientNotifier.KillUnit(unit.Id).ConfigureAwait(true);
	}

	private static async Task ShootWithDelay(Weapon weapon, ITaskDelay delay, CancellationToken cancellationToken)
	{
		var delayShot = (int)weapon.DelayShot.TotalMilliseconds;
		var rndDelay = RandomNumberGenerator.GetInt32(delayShot / 2, delayShot);
		await delay.Delay(TimeSpan.FromMilliseconds(rndDelay), cancellationToken).ConfigureAwait(true);
		weapon.Ammo -= 1;
	}

	private static async Task ReloadWithDelay(Weapon weapon, ITaskDelay delay, CancellationToken cancellationToken)
	{
		await delay.Delay(weapon.ReloadTime, cancellationToken).ConfigureAwait(true);
		weapon.Ammo = weapon.MagazineSize;
	}
}