namespace WorldWar.YandexClient.Interfaces;

public interface IYandexHubConnection : IAsyncDisposable
{
	public Task ConfigureHubConnection();
}