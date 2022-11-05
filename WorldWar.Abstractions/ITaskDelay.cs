namespace WorldWar.Abstractions;

public interface ITaskDelay
{
	public Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken);
}