using System.Globalization;
using System.Security.Cryptography;
using WorldWar.Abstractions;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Interfaces;

namespace WorldWar.Internal;

public class CombatService : ICombatService
{
	private readonly IMapStorage _mapStorage;
	private readonly IMovableService _movableService;
	private readonly ITaskDelay _taskDelay;
	private readonly IAuthUser _authUser;
	private readonly IYandexJsClientTransmitter _yandexJsClientTransmitter;

	public CombatService(IMapStorage mapStorage, IMovableService movableService, ITaskDelay taskDelay, IYandexJsClientTransmitter yandexJsClientTransmitter, IAuthUser authUser)
	{
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_yandexJsClientTransmitter = yandexJsClientTransmitter ?? throw new ArgumentNullException(nameof(yandexJsClientTransmitter));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
	}

	public async Task AttackUnit(Guid enemyGuid, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		var user = await _mapStorage.GetUnit(identity.GuidId).ConfigureAwait(true);
		var enemy = await _mapStorage.GetUnit(enemyGuid).ConfigureAwait(true);

		if (user.Weapon.WeaponType == WeaponTypes.Handguns && !user.IsWithinReach(enemy.CurrentLongitude, enemy.CurrentLatitude, user.Weapon.Distance))
		{
			await _movableService.StartMove(enemy.Id, cancellationToken,
				user.Weapon.Distance).ConfigureAwait(true);
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
				await _yandexJsClientTransmitter.PlaySound("sound", user.Weapon.ReloadSoundLocation).ConfigureAwait(true);
				await ReloadWithDelay(user.Weapon, _taskDelay, cancellationToken).ConfigureAwait(true);
			}
			else
			{
				var damage = user.GetDamage(enemy);
				await _yandexJsClientTransmitter.ShootUnit(user.Id, enemy.CurrentLatitude, enemy.CurrentLongitude).ConfigureAwait(true);
				await _yandexJsClientTransmitter.PlaySound("sound", user.Weapon.ShotSoundLocation).ConfigureAwait(true);
				var message = damage > 0 ? damage.ToString(NumberFormatInfo.CurrentInfo) : "missed!";
				await _yandexJsClientTransmitter.SendMessage(enemy.Id, message).ConfigureAwait(true);
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
			await _yandexJsClientTransmitter.KillUnit(unit.Id).ConfigureAwait(true);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}

		var trophies = new List<Item>(unit.Loot.Items)
		{
			unit.Weapon,
			unit.BodyProtection,
			unit.HeadProtection,
		};
		var box = new Box(Guid.NewGuid(), unit.CurrentLatitude, unit.CurrentLongitude, trophies);
		await _mapStorage.SetItem(box).ConfigureAwait(true);
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