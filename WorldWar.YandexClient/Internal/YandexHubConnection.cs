using Microsoft.AspNetCore.SignalR.Client;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Internal
{
	internal class YandexHubConnection : IYandexHubConnection
	{
		private HubConnection? _hubConnection;
		private readonly IYandexJsClientAdapter yandexJsClientAdapter;

		public YandexHubConnection(IYandexJsClientAdapter yandexJsClientAdapter)
		{
			this.yandexJsClientAdapter = yandexJsClientAdapter;
		}

		public async Task ConfigureHubConnection(Uri uri)
		{
			_hubConnection = new HubConnectionBuilder()
				.WithUrl(uri)
				.Build();

			_hubConnection.On<Guid, float, float>("ShootUnit", async (id, latitude, longitude) =>
			{
				Console.WriteLine($"Shoot: {id}, Latitude:{latitude} ,Longitude:{longitude}");
				await yandexJsClientAdapter.ShootUnit(id, latitude, longitude);
			});

			_hubConnection.On<Guid, float, float>("RotateUnit", async (id, latitude, longitude) =>
			{
				Console.WriteLine($"RotateUnit: {id}, Latitude:{latitude} ,Longitude:{longitude}");
				await yandexJsClientAdapter.RotateUnit(id, latitude, longitude);
			});

			_hubConnection.On<Guid>("KillUnit", async (id) =>
			{
				Console.WriteLine($"KillUnit: {id}");
				await yandexJsClientAdapter.KillUnit(id);
			});

			_hubConnection.On<Guid, string>("SendMessage", async (id, message) =>
			{
				Console.WriteLine($"ShowMessage: {id}, Message: {message}");
				await yandexJsClientAdapter.ShowMessage(id, message);
			});

			_hubConnection.On<string, string>("PlaySound", async (elementId, src) =>
			{
				Console.WriteLine($"PlaySound: {elementId}, src: {src}");
				await yandexJsClientAdapter.PlaySound(elementId, src);
			});

			await _hubConnection.StartAsync();
		}

		public async ValueTask DisposeAsync()
		{
			if (_hubConnection is not null)
			{
				await _hubConnection.DisposeAsync();
			}
		}
	}
}
