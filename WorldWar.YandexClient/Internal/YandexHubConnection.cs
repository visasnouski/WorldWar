using Microsoft.AspNetCore.SignalR.Client;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Internal
{
	internal class YandexHubConnection : IYandexHubConnection
	{
		private HubConnection? _hubConnection;
		private readonly IYandexJsClientAdapter _yandexJsClientAdapter;

		public YandexHubConnection(IYandexJsClientAdapter yandexJsClientAdapter)
		{
			this._yandexJsClientAdapter =
				yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
		}

		public async Task ConfigureHubConnection(Uri uri)
		{
			_hubConnection = new HubConnectionBuilder()
				.WithUrl(uri)
				.WithAutomaticReconnect()
				.Build();

			_hubConnection.On<Guid, float, float>("ShootUnit", async (id, latitude, longitude) =>
			{
				Console.WriteLine($"Shoot: {id}, Latitude:{latitude} ,Longitude:{longitude}");
				await _yandexJsClientAdapter.ShootUnit(id, latitude, longitude).ConfigureAwait(true);
			});

			_hubConnection.On<Guid, float, float>("RotateUnit", async (id, latitude, longitude) =>
			{
				Console.WriteLine($"RotateUnit: {id}, Latitude:{latitude} ,Longitude:{longitude}");
				await _yandexJsClientAdapter.RotateUnit(id, latitude, longitude).ConfigureAwait(true);
			});

			_hubConnection.On<Guid>("KillUnit", async (id) =>
			{
				Console.WriteLine($"KillUnit: {id}");
				await _yandexJsClientAdapter.KillUnit(id).ConfigureAwait(true);
			});

			_hubConnection.On<Guid, string>("SendMessage", async (id, message) =>
			{
				Console.WriteLine($"ShowMessage: {id}, Message: {message}");
				await _yandexJsClientAdapter.ShowMessage(id, message).ConfigureAwait(true);
			});

			_hubConnection.On<string, string>("PlaySound", async (elementId, src) =>
			{
				Console.WriteLine($"PlaySound: {elementId}, src: {src}");
				await _yandexJsClientAdapter.PlaySound(elementId, src).ConfigureAwait(true);
			});

			await _hubConnection.StartAsync().ConfigureAwait(true);
		}

		public async ValueTask DisposeAsync()
		{
			if (_hubConnection is not null)
			{
				await _hubConnection.DisposeAsync().ConfigureAwait(true);
			}
		}
	}
}
