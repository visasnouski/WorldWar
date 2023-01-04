using WorldWar.Abstractions.Models.Units;

namespace WorldWar.Core.Interfaces;

public interface ICombatService
{
	public Task AttackUnit(Unit user, Unit enemy, CancellationToken cancellationToken);
}