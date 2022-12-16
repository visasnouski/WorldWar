using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Abstractions.Interfaces;

public interface IUnitManagementService
{
	Task MoveUnit(Unit unit, float[][] route);

	Task MoveUnit(Unit unit, float latitude, float longitude);

	Task Attack(Unit unit, Unit enemy);


	Task GetInCar(Guid unitId, Guid itemGuid);

    Task PickUp(Guid unitId, Guid itemGuid, bool isUnit);

    Task StopUnit(Guid unitId);
}