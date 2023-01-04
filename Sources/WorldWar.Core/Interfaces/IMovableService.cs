using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Core.Interfaces;

public interface IMovableService
{
	public Task StartMove(Unit unit, float[][] route, CancellationToken cancellationToken);

	public Task StartMove(Unit unit, Unit targetUnit, CancellationToken cancellationToken, float? distance = null);

	public Task Rotate(Unit unit, float latitude, float longitude, CancellationToken cancellationToken);
}