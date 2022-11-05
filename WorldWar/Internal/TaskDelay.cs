using WorldWar.Abstractions;

namespace WorldWar.Internal;

public class TaskDelay : ITaskDelay
{
    public async Task Delay(TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(timeSpan, cancellationToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}