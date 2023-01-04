using Microsoft.AspNetCore.SignalR.Client;
using WorldWar.YandexClient.Hubs;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Internal;

internal sealed class YandexJsClientNotifier : IYandexJsClientNotifier, IDisposable
{
	private readonly Lazy<Task<HubConnection>> _hubConnection;
	private bool _isDisposed;

	public YandexJsClientNotifier(IHttpBaseUrlAccessor httpBaseUrlAccessor)
	{
		_hubConnection = new Lazy<Task<HubConnection>>(async () =>
		{
			var hubConnection = new HubConnectionBuilder()
				.WithUrl(httpBaseUrlAccessor.GetUri())
				.Build();

			await hubConnection.StartAsync();
			return hubConnection;
		});
	}

	public async Task KillUnit(Guid id)
	{
		var hubConnection = await _hubConnection.Value;
		await hubConnection.SendAsync(nameof(YandexMapHub.SendKillUnit), id);
	}

	public async Task AttackUnit(Guid id, float enemyLatitude, float enemyLongitude)
	{
		var hubConnection = await _hubConnection.Value;
		await hubConnection.SendAsync(nameof(YandexMapHub.SendAttackUnit), id, enemyLatitude, enemyLongitude);
	}

	public async Task SendMessage(Guid id, string message)
	{
		var hubConnection = await _hubConnection.Value;
		await hubConnection.SendAsync(nameof(YandexMapHub.SendMessage), id, message);
	}

	public async Task PlaySound(string id, string src)
	{
		var hubConnection = await _hubConnection.Value;
		await hubConnection.SendAsync(nameof(YandexMapHub.SendPlaySound), id, src);
	}

	public async Task RotateUnit(Guid id, float latitude, float longitude)
	{
		var hubConnection = await _hubConnection.Value;
		await hubConnection.SendAsync(nameof(YandexMapHub.SendRotateUnit), id, latitude, longitude);
	}

	public void Dispose()
	{
		Dispose(true);
	}

	private void Dispose(bool disposing)
	{
		if (_isDisposed)
		{
			return;
		}

		if (disposing
			&& _hubConnection.IsValueCreated
			&& _hubConnection.Value.IsCompleted)
		{
			_hubConnection.Value.Dispose();
		}

		_isDisposed = true;
	}
}