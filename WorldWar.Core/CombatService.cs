using System.Globalization;
using System.Security.Cryptography;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Core;

internal class CombatService : ICombatService
{
	private readonly IMapStorage _mapStorage;
	private readonly IMovableService _movableService;
	private readonly ITasksStorage _tasksStorage;
	private readonly ITaskDelay _taskDelay;
	private readonly IAuthUser _authUser;
	private readonly IYandexJsClientNotifier _yandexJsClientNotifier;

	public CombatService(IMapStorage mapStorage, IMovableService movableService, ITasksStorage tasksStorage, ITaskDelay taskDelay, IYandexJsClientNotifier yandexJsClientNotifier, IAuthUser authUser)
	{
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_tasksStorage = tasksStorage ?? throw new ArgumentNullException(nameof(tasksStorage));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_yandexJsClientNotifier = yandexJsClientNotifier ?? throw new ArgumentNullException(nameof(yandexJsClientNotifier));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
	}

	public async Task AttackUnit(Guid enemyGuid, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		var user = await _mapStorage.GetUnit(identity.GuidId).ConfigureAwait(true);
		var enemy = await _mapStorage.GetUnit(enemyGuid).ConfigureAwait(true);

		if (user.Weapon.WeaponType == WeaponTypes.Handguns && !user.IsWithinReach(enemy.CurrentLongitude, enemy.CurrentLatitude, user.Weapon.Distance))
		{
			await _movableService.StartMove(user.Id, enemy.Id, cancellationToken, user.Weapon.Distance).ConfigureAwait(true);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		await _movableService.Rotate(user.Id, enemy.CurrentLatitude, enemy.CurrentLongitude, cancellationToken).ConfigureAwait(true);

		while (!cancellationToken.IsCancellationRequested)
		{
			enemy = await _mapStorage.GetUnit(enemyGuid).ConfigureAwait(true);
			user.RotateUnit(enemy.CurrentLongitude, enemy.CurrentLatitude);

			if (user.Weapon.Ammo <= 0)
			{
				await _yandexJsClientNotifier.PlaySound("sound", user.Weapon.ReloadSoundLocation).ConfigureAwait(true);
				await ReloadWithDelay(user.Weapon, _taskDelay, cancellationToken).ConfigureAwait(true);
			}
			else
			{
				var damage = user.GetDamage(enemy);
				await _yandexJsClientNotifier.ShootUnit(user.Id, enemy.CurrentLatitude, enemy.CurrentLongitude).ConfigureAwait(true);
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

			await _mapStorage.SetUnit(enemy).ConfigureAwait(true);

			if (user.Health <= 0)
			{
				await KillUnit(user).ConfigureAwait(true);
				break;
			}

			await _mapStorage.SetUnit(user).ConfigureAwait(true);
		}
	}

	private async Task KillUnit(Unit unit)
	{
		try
		{
			if (_tasksStorage.TryGetValue(unit.Id, out var task))
			{
				task!.Value.Item1.Cancel();
			}
			_tasksStorage.TryRemove(unit.Id);

			await _yandexJsClientNotifier.KillUnit(unit.Id).ConfigureAwait(true);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
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