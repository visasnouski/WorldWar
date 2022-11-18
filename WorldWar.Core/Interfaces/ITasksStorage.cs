namespace WorldWar.Core.Interfaces;

public interface ITasksStorage
{
    public void AddOrUpdate(Guid unitId, (CancellationTokenSource, Task) task);

    public bool TryGetValue(Guid unitId, out (CancellationTokenSource, Task)? task);

    public void TryRemove(Guid unitId);
}