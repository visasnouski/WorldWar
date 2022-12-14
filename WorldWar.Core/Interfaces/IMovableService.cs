namespace WorldWar.Core.Interfaces;

public interface IMovableService
{
	public Task StartMove(Guid unitId, float[][] route, CancellationToken cancellationToken);

	public Task StartMove(Guid unitId, Guid targetGuid, CancellationToken cancellationToken, float? distance = null);

	public Task Rotate(Guid unitId, float latitude, float longitude, CancellationToken cancellationToken);
}