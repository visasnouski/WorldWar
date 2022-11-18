namespace WorldWar.Core.Interfaces;

public interface IMovableService
{
    public Task StartMoveAlongRoute(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken, float? weaponDistance = null);

    public Task StartMoveToCoordinates(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken, float? weaponDistance = null);

    public Task StartMove(Guid unitId, Guid targetGuid, CancellationToken cancellationToken, float? distance = null);

    public Task Rotate(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken);
}