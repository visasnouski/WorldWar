namespace WorldWar.YandexClient.Interfaces;

/// <summary>
/// Performs an actions on the map for all players.
/// Uses the SignalR to transmit commands.
/// </summary>
public interface IYandexJsClientNotifier
{
	/// <summary>
	/// Runs the animation of the unit's death
	/// </summary>
	/// <param name="id">Globally unique identifier of unit</param>
	/// <returns></returns>
	public Task KillUnit(Guid id);

	/// <summary>
	/// Runs the animation of the unit's shot
	/// </summary>
	/// <param name="id">Globally unique identifier of unit</param>
	/// <param name="enemyLatitude"></param>
	/// <param name="enemyLongitude"></param>
	/// <returns></returns>
	public Task ShootUnit(Guid id, float enemyLatitude, float enemyLongitude);

	/// <summary>
	/// Rotates the unit according to the coordinates
	/// </summary>
	/// <param name="id"></param>
	/// <param name="latitude"></param>
	/// <param name="longitude"></param>
	/// <returns></returns>
	public Task RotateUnit(Guid id, float latitude, float longitude);

	/// <summary>
	/// Displays a message above the unit
	/// </summary>
	/// <param name="id">Globally unique identifier of unit</param>
	/// <param name="message"></param>
	/// <returns></returns>
	public Task SendMessage(Guid id, string message);

	/// <summary>
	/// Plays the sound
	/// </summary>
	/// <param name="id">Id of the HTML element audio</param>
	/// <param name="src">The URL of the audio to plays</param>
	/// <returns></returns>
	public Task PlaySound(string id, string src);
}
