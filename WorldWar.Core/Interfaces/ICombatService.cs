namespace WorldWar.Core.Interfaces;

public interface ICombatService
{
	public Task AttackUnit(Guid userGuid, Guid enemyGuid, CancellationToken cancellationToken);
}