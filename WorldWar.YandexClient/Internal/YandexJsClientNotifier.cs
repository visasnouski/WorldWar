using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using WorldWar.YandexClient.Hubs;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Internal;

internal sealed class YandexJsClientNotifier : IYandexJsClientNotifier, IDisposable
{
	private readonly Lazy<Task<HubConnection>> _hubConnection;
	private bool _isDisposed;

	public YandexJsClientNotifier(NavigationManager navigationManager)
	{
		_hubConnection = new Lazy<Task<HubConnection>>(async () =>
		{
			var hubConnection = new HubConnectionBuilder()
				.WithUrl(navigationManager.ToAbsoluteUri("/yandexMapHub"))
				.Build();

			await hubConnection.StartAsync().ConfigureAwait(false);
			return hubConnection;
		});
	}

	public async Task KillUnit(Guid id)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(false);
		await hubConnection.SendAsync(nameof(YandexMapHub.SendKillUnit), id).ConfigureAwait(false);
	}

	public async Task ShootUnit(Guid id, float enemyLatitude, float enemyLongitude)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(false);
		await hubConnection.SendAsync(nameof(YandexMapHub.SendShootUnit), id, enemyLatitude, enemyLongitude).ConfigureAwait(false);
	}

	public async Task SendMessage(Guid id, string message)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(false);
		await hubConnection.SendAsync(nameof(YandexMapHub.SendMessage), id, message).ConfigureAwait(false);
	}

	public async Task PlaySound(string id, string src)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(false);
		await hubConnection.SendAsync(nameof(YandexMapHub.SendPlaySound), id, src).ConfigureAwait(false);
	}

	public async Task RotateUnit(Guid id, float latitude, float longitude)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(false);
		await hubConnection.SendAsync(nameof(YandexMapHub.SendRotateUnit), id, latitude, longitude).ConfigureAwait(false);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
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