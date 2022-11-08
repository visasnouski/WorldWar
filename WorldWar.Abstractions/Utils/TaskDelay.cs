using WorldWar.Abstractions.Interfaces;

namespace WorldWar.Abstractions.Utils;

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