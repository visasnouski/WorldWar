namespace WorldWar.YandexClient.Interfaces;

public interface IYandexJsClientTransmitter
{
	public Task KillUnit(Guid id);

	public Task ShootUnit(Guid id, float enemyLatitude, float enemyLongitude);

	public Task RotateUnit(Guid id, float latitude, float longitude);

	public Task SendMessage(Guid id, string message);

	public Task PlaySound(string id, string src);
}
