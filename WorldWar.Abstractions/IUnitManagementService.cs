namespace WorldWar.Abstractions
{
	public interface IUnitManagementService
	{
		Task MoveUnit(Guid unitId, float latitude, float longitude, bool useRoute = false);

		Task StopUnit(Guid unitId);

		Task Attack(Guid unitId, Guid enemyGuid);

		Task GetInCar(Guid unitId, Guid itemGuid);

		Task PickUp(Guid unitId, Guid itemGuid, bool isUnit);
	}
}