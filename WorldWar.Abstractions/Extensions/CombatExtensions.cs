using System.Numerics;
using System.Security.Cryptography;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Abstractions.Extensions;

public static class CombatExtensions
{
	public static int CalculateDamage(this Unit user, Unit enemy)
	{
		var distance = Vector2.Distance(user.Location.CurrentPos, enemy.Location.CurrentPos);

		var ratioDistanceToLocation = user.Weapon.Distance / 2 / distance;
		var random = RandomNumberGenerator.GetInt32(1, 100);
		if (random > user.Weapon.Accuracy * ratioDistanceToLocation)
		{
			// Missed
			return 0;
		}

		var rndDamage = (float)RandomNumberGenerator.GetInt32(1, user.Weapon.Damage + 1);
		var damage = (int)(rndDamage * (user.Weapon.Accuracy * ratioDistanceToLocation / 100));
		return damage;
	}
}