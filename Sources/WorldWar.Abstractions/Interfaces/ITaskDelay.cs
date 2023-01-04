namespace WorldWar.Abstractions.Interfaces;

public interface ITaskDelay
{
	public Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken);
}