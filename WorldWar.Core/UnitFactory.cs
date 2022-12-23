using Microsoft.Extensions.Logging;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Core
{
	internal class UnitFactory : IUnitFactory
	{
		private readonly INotifier _notifier;
		private readonly ILogger<UnitFactory> _logger;

		public UnitFactory(INotifier notifier, ILogger<UnitFactory> logger)
		{
			_notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public Unit Create(UnitTypes type, Guid id, string userName, float latitude, float longitude, int health, Weapon? weapon = null, HeadProtection? headProtection = null, BodyProtection? bodyProtection = null, Loot? loot = null)
		{
			var unit = GetUnit(type, id, userName, latitude, longitude, health, weapon, headProtection, bodyProtection, loot);

			unit.AddNotifier(_notifier);

			_logger.LogInformation("The new unit {id} was created: [ type:{unitType}, lat:{latitude}, lon: {longitude}]",
				unit.Id, unit.UnitType, unit.Latitude, unit.Longitude);
			return unit;
		}

		private static Unit GetUnit(UnitTypes type, Guid id, string userName, float latitude, float longitude, int health,
			Weapon? weapon, HeadProtection? headProtection, BodyProtection? bodyProtection, Loot? loot)
		{
			return type switch
			{
				UnitTypes.Player => new Player(id, userName, latitude, longitude, health, weapon, headProtection,
					bodyProtection, loot),
				UnitTypes.Mob => new Bot(id, userName, type, latitude, longitude, health, weapon, headProtection,
					bodyProtection, loot),
				UnitTypes.Car => new Car(id, userName, latitude, longitude, health, weapon, headProtection, bodyProtection,
					loot),
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown unit type")
			};
		}
	}
}
