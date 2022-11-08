using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Internal;

internal class YandexJsClientTransmitter : IYandexJsClientTransmitter, IDisposable
{
	private readonly Lazy<Task<HubConnection>> _hubConnection;
	private bool _isDisposed;

	public YandexJsClientTransmitter(NavigationManager navigationManager)
	{
		_hubConnection = new Lazy<Task<HubConnection>>(async () =>
		{
			var hubConnection = new HubConnectionBuilder()
				.WithUrl(navigationManager.ToAbsoluteUri("/yandexMapHub"))
				.Build();

			await hubConnection.StartAsync().ConfigureAwait(true);
			return hubConnection;
		});
	}

	public async Task KillUnit(Guid id)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(true);
		await hubConnection.SendAsync("SendKillUnit", id).ConfigureAwait(true);
	}

	public async Task ShootUnit(Guid id, float enemyLatitude, float enemyLongitude)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(true);
		await hubConnection.SendAsync("SendShootUnit", id, enemyLatitude, enemyLongitude).ConfigureAwait(true);
	}

	public async Task SendMessage(Guid id, string message)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(true);
		await hubConnection.SendAsync("SendMessage", id, message).ConfigureAwait(true);
	}

	public async Task PlaySound(string id, string src)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(true);
		await hubConnection.SendAsync("SendPlaySound", id, src).ConfigureAwait(true);
	}

	public async Task RotateUnit(Guid id, float latitude, float longitude)
	{
		var hubConnection = await _hubConnection.Value.ConfigureAwait(true);
		await hubConnection.SendAsync("SendRotateUnit", id, latitude, longitude).ConfigureAwait(true);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
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