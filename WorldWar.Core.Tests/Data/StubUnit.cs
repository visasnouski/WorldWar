using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Core.Tests.Data
{
	internal class StubUnit : Unit
	{
		public StubUnit(Guid guid, int health = 100)
			: base(guid, "FakeUnit", UnitTypes.Mob, 10.00f, 20.00f, health)
		{
		}

		public override float Speed => 0.01f;
	}
}
