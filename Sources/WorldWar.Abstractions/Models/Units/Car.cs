using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Models.Units;

public class Car : Unit
{
	public override float Speed => 00.00009F;

	private Unit? _driver;

	public override Guid Id => _driver?.Id ?? base.Id;

	public Car(Guid id, string name, float latitude, float longitude, int health, Weapon? weapon = null, HeadProtection? headProtection = null, BodyProtection? bodyProtection = null, Loot? loot = null)
		: base(id, name, UnitTypes.Car, latitude, longitude, health, weapon, headProtection, bodyProtection, loot)
	{
	}

	public void GetBehindWheel(Unit unit)
	{
		_driver = unit;
	}

	public bool TryGetOffWheel(out Unit? unit)
	{
		unit = null;
		if (_driver == null)
		{
			return false;
		}

		unit = _driver;

		unit.Location.ChangeLocation(Latitude, Longitude);
		unit.SaveCurrentLocation();
		_driver = null;
		return true;
	}
}