namespace WorldWar.Abstractions.Interfaces;

public interface IUserManagement
{
    Task AddUnit();

    Task MoveUnit(float latitude, float longitude);

    Task StopUnit();

    Task Attack(Guid enemyGuid);

    Task GetInCar(Guid itemGuid);

    Task PickUp(Guid itemGuid, bool isUnit);
}