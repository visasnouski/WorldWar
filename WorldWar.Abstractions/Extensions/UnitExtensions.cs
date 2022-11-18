using System.Numerics;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Abstractions.Extensions
{
	public static class UnitExtensions
	{
		public static void RotateUnit(this Unit unit, float endLongitude, float endLatitude, float? fromLongitude = null, float? fromLatitude = null)
		{
			if (unit == null)
			{
				throw new ArgumentNullException(nameof(unit));
			}

			fromLongitude ??= unit.Location.StartPos.X;
			fromLatitude ??= unit.Location.StartPos.Y;
			unit.Rotate = -90 + BearingCalculator.Calculate(fromLatitude.Value, fromLongitude.Value, endLatitude, endLongitude);
		}

		public static bool IsWithinReach(this Unit unit, float endLongitude, float endLatitude, float? weaponDistance = null)
		{
			if (unit == null)
			{
				throw new ArgumentNullException(nameof(unit));
			}

			weaponDistance ??= unit.Speed * 10;
			var currentDistance = Vector2.Distance(unit.Location.CurrentPos, new Vector2(endLongitude, endLatitude));
			return currentDistance < weaponDistance;
		}

		public static void AddDamage(this Unit unit,int damage)
		{
			if (unit == null)
			{
				throw new ArgumentNullException(nameof(unit));
			}

			var protection = unit.HeadProtection.Defense + unit.BodyProtection.Defense;
			var realDamage = damage - damage * protection / 100;
			unit.Health -= realDamage;
		}

		public static void SetWeapon(this Unit unit, Weapon weapon)
		{
			if (unit == null)
			{
				throw new ArgumentNullException(nameof(unit));
			}

			unit.Loot.Items.Add(unit.Weapon);
			unit.Weapon = weapon;
		}

		public static void SetBodyProtection(this Unit unit, BodyProtection bodyProtection)
		{
			if (unit == null)
			{
				throw new ArgumentNullException(nameof(unit));
			}

			unit.Loot.Items.Add(unit.BodyProtection);
			unit.BodyProtection = bodyProtection;
		}

		public static void SetHeadProtection(this Unit unit, HeadProtection headProtection)
		{
			if (unit == null)
			{
				throw new ArgumentNullException(nameof(unit));
			}

			unit.Loot.Items.Add(unit.HeadProtection);
			unit.HeadProtection = headProtection;
		}
		
		public static void ChangeUnitType(this Unit unit, UnitTypes unitType)
		{
			if (unit == null)
			{
				throw new ArgumentNullException(nameof(unit));
			}

			unit.UnitType = unitType;
		}


		public static void SaveCurrentLocation(this Unit unit)
		{
			unit.Location.SaveCurrentLocation();
		}
	}
}
