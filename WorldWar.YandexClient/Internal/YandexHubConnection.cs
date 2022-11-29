using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Internal;

internal class YandexHubConnection : IYandexHubConnection
{
	private HubConnection? _hubConnection;
	private readonly IYandexJsClientAdapter _yandexJsClientAdapter;
	private readonly ILogger<YandexHubConnection> _logger;

	public YandexHubConnection(IYandexJsClientAdapter yandexJsClientAdapter, ILogger<YandexHubConnection> logger)
	{
		this._yandexJsClientAdapter =
			yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task ConfigureHubConnection(Uri uri)
	{
		_hubConnection = new HubConnectionBuilder()
			.WithUrl(uri)
			.WithAutomaticReconnect()
			.Build();

		_hubConnection.On<Guid, float, float>("ShootUnit", async (id, latitude, longitude) =>
		{
			_logger.LogInformation($"Shoot: {id}, Latitude:{latitude} ,Longitude:{longitude}");
			await _yandexJsClientAdapter.ShootUnit(id, latitude, longitude).ConfigureAwait(true);
		});

		_hubConnection.On<Guid, float, float>("RotateUnit", async (id, latitude, longitude) =>
		{
			_logger.LogInformation($"RotateUnit: {id}, Latitude:{latitude} ,Longitude:{longitude}");
			await _yandexJsClientAdapter.RotateUnit(id, latitude, longitude).ConfigureAwait(true);
		});

		_hubConnection.On<Guid>("KillUnit", async (id) =>
		{
			_logger.LogInformation($"KillUnit: {id}");
			await _yandexJsClientAdapter.KillUnit(id).ConfigureAwait(true);
		});

		_hubConnection.On<Guid, string>("SendMessage", async (id, message) =>
		{
			_logger.LogInformation($"ShowMessage: {id}, Message: {message}");
			await _yandexJsClientAdapter.ShowMessage(id, message).ConfigureAwait(true);
		});

		_hubConnection.On<string, string>("PlaySound", async (elementId, src) =>
		{
			_logger.LogInformation($"PlaySound: {elementId}, src: {src}");
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