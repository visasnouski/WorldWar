using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.YandexClient.Tests.Internal.Data
{
	internal class StubUnit : Unit
	{
		public StubUnit(UnitTypes? unitType = null)
			: base(Guid.Parse("3B3381B0-4CAB-4159-B68C-94613CD40D96"), "FakeUnit", unitType ?? UnitTypes.Player, 10.00f, 20.00f, 100)
		{
		}

		public override float Speed => 0.01f;
	}
}
