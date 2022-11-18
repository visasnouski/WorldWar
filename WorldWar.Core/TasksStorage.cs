using System.Collections.Concurrent;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core;

public class TasksStorage : ITasksStorage
{
	// Added due to the need to manage a unit on multiple open tabs
	// The CancellationToken is used to stop task of the current user.
	private readonly ConcurrentDictionary<Guid, (CancellationTokenSource, Task)> _storage = new();

	public void AddOrUpdate(Guid unitId, (CancellationTokenSource, Task) task)
	{
		_storage.AddOrUpdate(unitId, task,
			(_, innerTask) =>
			{
				innerTask.Item1.Cancel(false);
				return task;
			});
	}

	public bool TryGetValue(Guid unitId, out (CancellationTokenSource, Task)? task)
	{
		task = null;
		if (_storage.TryGetValue(unitId, out var innerTask))
		{
			task = innerTask;
			return true;
		}

		return false;
	}

	public void TryRemove(Guid unitId)
	{
		_storage.TryRemove(unitId, out _);
	}
}