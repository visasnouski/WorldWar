using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Models.Units;

public class Player : Unit
{
	public override float Speed => 00.00001F;

	public Player(Guid id, string userName, float latitude, float longitude, int health, Weapon? weapon = null, HeadProtection? headProtection = null, BodyProtection? bodyProtection = null,
        Loot? loot = null)
        : base(id, userName, UnitTypes.Player, latitude, longitude, health, weapon, headProtection, bodyProtection, loot)
    {
    }
}