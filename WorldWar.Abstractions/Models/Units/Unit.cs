using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Models.Units;

public abstract class Unit
{
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
}
