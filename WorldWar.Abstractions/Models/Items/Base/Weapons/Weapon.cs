using System.Security.Cryptography;

namespace WorldWar.Abstractions.Models.Items.Base.Weapons;

public class Weapon : Item
{
	public override int Id { get; init; }

	public override string Name { get; init; } = null!;

	public override ItemTypes ItemType { get; init; }

	public WeaponTypes WeaponType { get; init; }

	public float Distance { get; init; }

	public int Damage { get; init; }

	public int MagazineSize { get; init; }

	public int Ammo { get; set; }

	public int Accuracy { get; init; }

	public TimeSpan DelayShot { get; init; }

	public TimeSpan ReloadTime { get; init; }

	public string ShotSoundLocation { get; init; } = null!;

	public string ReloadSoundLocation { get; init; } = null!;

	public override string IconPath { get; init; } = null!;

	public override int Size => WeaponType is WeaponTypes.Rifles or WeaponTypes.Shotguns ? 2 : 1;

	public int CalculateDamage(float distance)
	{
		var ratioDistanceToLocation = (distance != 0) ? Distance / 2 / distance : 1;
		var random = RandomNumberGenerator.GetInt32(1, 100);
		if (random > Accuracy * ratioDistanceToLocation)
		{
			// Missed
			return 0;
		}

		var rndDamage = (float)RandomNumberGenerator.GetInt32(1, Damage + 1);
		var damage = (int)(rndDamage * (Accuracy * ratioDistanceToLocation / 100));
		return damage;
	}
}