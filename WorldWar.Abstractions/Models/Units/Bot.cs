using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Models.Units;
public class Bot : Unit
{
	public Bot(Guid id, string botName, UnitTypes unitTypes, float latitude, float longitude, int health, Weapon? weapon = null, HeadProtection? headProtection = null, BodyProtection? bodyProtection = null, Loot? loot = null)
		: base(id, botName, unitTypes, latitude, longitude, health, weapon, headProtection, bodyProtection, loot)
	{
	}
}