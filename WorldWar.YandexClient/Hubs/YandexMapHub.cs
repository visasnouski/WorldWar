using Microsoft.AspNetCore.SignalR;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Hubs;

public class YandexMapHub : Hub<IYandexJsClientNotifier>
{
	public async Task SendShootUnit(Guid id, float enemyLatitude, float enemyLongitude)
	{
		await Clients.All.ShootUnit(id, enemyLatitude, enemyLongitude).ConfigureAwait(false);
	}

	public async Task SendRotateUnit(Guid id, float latitude, float longitude)
	{
		await Clients.All.RotateUnit(id, latitude, longitude).ConfigureAwait(false);
	}

	public async Task SendKillUnit(Guid id)
	{
		await Clients.All.KillUnit(id).ConfigureAwait(false);
	}

	public async Task SendMessage(Guid id, string message)
	{
		await Clients.All.SendMessage(id, message).ConfigureAwait(false);
	}

	public async Task SendPlaySound(string elementId, string src)
	{
		await Clients.All.PlaySound(elementId, src).ConfigureAwait(false);
	}
}