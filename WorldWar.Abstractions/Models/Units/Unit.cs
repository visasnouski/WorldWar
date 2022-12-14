using System.Globalization;
using System.Security.Cryptography;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Models.Units;

public abstract class Unit
{
	private Func<Guid, string, Task>? _damageNotificationFunc;
	private Func<Guid, Task>? _deathNotificationFunc;
	private Func<Guid, float, float, Task>? _rotateNotificationFunc;
	private Func<string, string, Task>? _soundNotificationFunc;
	private Func<Guid, float, float, Task>? _shootNotificationFunc;

	public string Name { get; init; }

	public Guid Id { get; init; }

	public UnitTypes UnitType { get; set; }

	public int Health { get; set; }

	public Weapon Weapon { get; set; }

	public HeadProtection HeadProtection { get; set; }

	public BodyProtection BodyProtection { get; set; }

	public double Rotate { get; set; }

	public Loot Loot { get; init; }

	public float Latitude => Location.CurrentPos.Y;

	public float Longitude => Location.CurrentPos.X;

	public abstract float Speed { get; }

	public float ViewingDistance => 00.0015F;

	public Location Location { get; init; }

	protected Unit(Guid id,
		string name,
		UnitTypes unitType,
		float latitude,
		float longitude,
		int health,
		Weapon? weapon = null,
		HeadProtection? headProtection = null,
		BodyProtection? bodyProtection = null,
		Loot? loot = null)
	{
		Id = id;
		Name = name;
		UnitType = unitType;
		Location = new Location(longitude, latitude);
		Health = health;
		//TODO implement values by default
		Weapon = weapon ?? WeaponModels.Fist;
		HeadProtection = headProtection ?? HeadProtectionModels.Bandana;
		BodyProtection = bodyProtection ?? BodyProtectionModels.WifeBeater;
		Loot = loot ?? new Loot() { Id = id.GetHashCode(), Items = new List<Item>() };
	}

	public string MobTypesString => UnitType.ToString();

	public void AddDamageNotifier(Func<Guid, string, Task> damageNotificationFunc)
	{
		_damageNotificationFunc = damageNotificationFunc;
	}

	public void AddRotateNotifier(Func<Guid, float, float, Task> rotateNotificationFunc)
	{
		_rotateNotificationFunc = rotateNotificationFunc;
	}

	public void AddDeathNotifier(Func<Guid, Task> deathNotificationFunc)
	{
		_deathNotificationFunc = deathNotificationFunc;
	}

	public void AddSoundNotifier(Func<string, string, Task> soundNotificationFunc)
	{
		_soundNotificationFunc = soundNotificationFunc;
	}

	public void AddShootNotifier(Func<Guid, float, float, Task> shootNotificationFunc)
	{
		_shootNotificationFunc = shootNotificationFunc;
	}

	public async Task RotateUnit(float endLongitude, float endLatitude, float? fromLongitude = null, float? fromLatitude = null)
	{
		fromLongitude ??= Location.StartPos.X;
		fromLatitude ??= Location.StartPos.Y;
		Rotate = -90 + BearingCalculator.Calculate(fromLatitude.Value, fromLongitude.Value, endLatitude, endLongitude);

		if (_rotateNotificationFunc != null)
		{
			await _rotateNotificationFunc.Invoke(Id, endLatitude, endLongitude);
		}
	}

	public async Task Shoot(Unit enemy, ITaskDelay taskDelay, CancellationToken cancellationToken)
	{
		await RotateUnit(enemy.Longitude, enemy.Latitude);
		var distance = Location.GetDistance(enemy.Location);

		if (Weapon.Ammo <= 0)
		{
			await PlaySound(Weapon.ReloadSoundLocation);
			await taskDelay.Delay(Weapon.ReloadTime, cancellationToken).ConfigureAwait(false);
			Weapon.Ammo = Weapon.MagazineSize;
		}

		var damage = Weapon.CalculateDamage(distance);

		if (_shootNotificationFunc != null)
		{
			await _shootNotificationFunc.Invoke(Id, enemy.Latitude, enemy.Longitude);
		}

		await PlaySound(Weapon.ShotSoundLocation);
		await enemy.AddDamage(damage);
		await ShootWithDelay(Weapon, taskDelay, cancellationToken);
	}

	public void Move(TimeSpan time, float endLongitude, float endLatitude)
	{
		Location.Move(time, endLongitude, endLatitude, Speed);
	}

	private async Task PlaySound(string src)
	{
		if (_soundNotificationFunc != null)
		{
			await _soundNotificationFunc.Invoke("sound", src);
		}
	}

	private static async Task ShootWithDelay(Weapon weapon, ITaskDelay delay, CancellationToken cancellationToken)
	{
		// Added randomness so that the interval between shots is different
		var delayShot = (int)weapon.DelayShot.TotalMilliseconds;
		var rndDelay = RandomNumberGenerator.GetInt32(delayShot / 2, delayShot);
		await delay.Delay(TimeSpan.FromMilliseconds(rndDelay), cancellationToken).ConfigureAwait(false);
		weapon.Ammo -= 1;
	}

	private async Task AddDamage(int damage)
	{
		var protection = HeadProtection.Defense + BodyProtection.Defense;
		var realDamage = damage - damage * protection / 100;

		if (_damageNotificationFunc != null)
		{
			var message = damage > 0 ? damage.ToString(NumberFormatInfo.CurrentInfo) : "missed!";
			await _damageNotificationFunc.Invoke(Id, message);
		}
		Health -= realDamage;

		if (_deathNotificationFunc != null && Health <= 0)
		{
			await _deathNotificationFunc.Invoke(Id);
		}
	}
}
