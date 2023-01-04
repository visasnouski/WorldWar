using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Internal;

internal class YandexHubConnection : IYandexHubConnection
{
	private HubConnection? _hubConnection;
	private readonly IYandexJsClientAdapter _yandexJsClientAdapter;
	private readonly IHttpBaseUrlAccessor _httpBaseUrlAccessor;
	private readonly ILogger<YandexHubConnection> _logger;

	public YandexHubConnection(IYandexJsClientAdapter yandexJsClientAdapter, IHttpBaseUrlAccessor httpBaseUrlAccessor, ILogger<YandexHubConnection> logger)
	{
		this._yandexJsClientAdapter = yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
		this._httpBaseUrlAccessor = httpBaseUrlAccessor ?? throw new ArgumentNullException(nameof(httpBaseUrlAccessor));
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task ConfigureHubConnection()
	{
		_hubConnection = new HubConnectionBuilder()
			.WithUrl(_httpBaseUrlAccessor.GetUri())
			.WithAutomaticReconnect()
			.Build();

		_hubConnection.On<Guid, float, float>("AttackUnit", async (id, latitude, longitude) =>
		{
			_logger.LogInformation("Shoot: {Id}, Latitude:{Latitude} ,Longitude:{Longitude}", id, latitude, longitude);
			await _yandexJsClientAdapter.ShootUnit(id, latitude, longitude);
		});

		_hubConnection.On<Guid, float, float>("RotateUnit", async (id, latitude, longitude) =>
		{
			_logger.LogInformation("Rotate unit: {Id}, Latitude:{Latitude} ,Longitude:{Longitude}", id, latitude, longitude);
			await _yandexJsClientAdapter.RotateUnit(id, latitude, longitude);
		});

		_hubConnection.On<Guid>("KillUnit", async (id) =>
		{
			_logger.LogInformation("Kill unit: {Id}", id);
			await _yandexJsClientAdapter.KillUnit(id);
		});

		_hubConnection.On<Guid, string>("SendMessage", async (id, message) =>
		{
			_logger.LogInformation("Show message: {Id}, Message: {Message}", id, message);
			await _yandexJsClientAdapter.ShowMessage(id, message);
		});

		_hubConnection.On<string, string>("PlaySound", async (elementId, src) =>
		{
			_logger.LogInformation("Play sound: {ElementId}, src: {Src}", elementId, src);
			await _yandexJsClientAdapter.PlaySound(elementId, src);
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