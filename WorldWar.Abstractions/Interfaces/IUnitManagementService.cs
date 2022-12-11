namespace WorldWar.Abstractions.Interfaces;

public interface IUnitManagementService
{
	Task MoveUnit(Guid unitId, float[][] route);

	Task MoveUnit(Guid unitId, float latitude, float longitude);

	Task Attack(Guid unitId, Guid enemyGuid);

    Task GetInCar(Guid unitId, Guid itemGuid);

    Task PickUp(Guid unitId, Guid itemGuid, bool isUnit);

    Task StopUnit(Guid unitId);
}