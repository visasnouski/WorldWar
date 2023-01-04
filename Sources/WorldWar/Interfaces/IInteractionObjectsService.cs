using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Interfaces;

public interface IInteractionObjectsService
{
	public Task PickUp(Unit unit, Unit targetUnit, CancellationToken cancellationToken);

	public Task PickUp(Unit unit, Box targetItem, CancellationToken cancellationToken);

	public Task GetIn(Unit unit, Unit targetUnit, CancellationToken cancellationToken);

	public Task GetOut(Unit unit, CancellationToken cancellationToken);
}