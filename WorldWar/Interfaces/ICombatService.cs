namespace WorldWar.Interfaces;

public interface ICombatService
{
	public Task AttackUnit(Guid enemyGuid, CancellationToken cancellationToken);
}