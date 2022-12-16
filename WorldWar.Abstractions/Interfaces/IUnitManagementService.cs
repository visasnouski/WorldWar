using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Abstractions.Interfaces;

public interface IUnitManagementService
{
	Task MoveUnit(Unit unit, float[][] route);

	Task MoveUnit(Unit unit, float latitude, float longitude);

	Task Attack(Unit unit, Unit enemy);

	Task GetInCar(Unit unit, Unit targetUnit);

	Task PickUp(Unit unit, Box targetItem);

	Task PickUp(Unit unit, Unit targetUnit);

	Task StopUnit(Guid unitId);
}