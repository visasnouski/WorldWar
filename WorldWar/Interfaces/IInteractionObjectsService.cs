namespace WorldWar.Interfaces;

public interface IInteractionObjectsService
{
	public Task PickUp(Guid guidId, bool isUnit, CancellationToken cancellationToken);

	public Task GetIn(Guid guidId, CancellationToken cancellationToken);

	public Task GetOut(Guid guidId, CancellationToken cancellationToken);
}