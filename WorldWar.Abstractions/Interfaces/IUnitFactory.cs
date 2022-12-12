using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Interfaces
{
    public interface IUnitFactory
    {
        public Unit Create(UnitTypes type, Guid id, string userName, float latitude, float longitude, int health,
            Weapon? weapon = null, HeadProtection? headProtection = null, BodyProtection? bodyProtection = null,
            Loot? loot = null);
    }
}
