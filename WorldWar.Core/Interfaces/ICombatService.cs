namespace WorldWar.Core.Interfaces;

public interface ICombatService
{
    public Task AttackUnit(Guid enemyGuid, CancellationToken cancellationToken);
}