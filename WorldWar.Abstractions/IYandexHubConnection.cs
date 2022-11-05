namespace WorldWar.Abstractions
{
	public interface IYandexHubConnection : IAsyncDisposable
	{
		public Task ConfigureHubConnection(Uri uri);
	}
}
