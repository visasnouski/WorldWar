using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Models.Units;

public class Car : Unit
{
	private readonly IList<Guid> _passengers;

	public Car(Guid id, string name, float latitude, float longitude, int health, Weapon? weapon = null, HeadProtection? headProtection = null, BodyProtection? bodyProtection = null, Loot? loot = null)
		: base(id, name, UnitTypes.Car, latitude, longitude, health,  weapon, headProtection, bodyProtection, loot)
	{
		_passengers = new List<Guid>();
	}

	public IList<Guid> Passengers => _passengers;

	public void AddPassengers(Guid guid)
	{
		_passengers.Add(guid);
	}
}