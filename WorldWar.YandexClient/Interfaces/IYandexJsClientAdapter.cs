using Microsoft.JSInterop;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.YandexClient.Interfaces;

/// <summary>
/// Performs actions on the map for the current player
/// </summary>
public interface IYandexJsClientAdapter
{
	/// <summary>
	/// Adds a unit to the map
	/// </summary>
	/// <param name="unit"></param>
	/// <returns></returns>
	public Task AddUnit(Unit unit);

	/// <summary>
	/// Updates an unit on the map
	/// </summary>
	/// <param name="unit"></param>
	/// <returns></returns>
	public Task UpdateUnit(Unit unit);

	/// <summary>
	/// Removes objects from the map
	/// </summary>
	/// <param name="ids">Globally unique identifier of units</param>
	/// <returns></returns>
	public Task RemoveGeoObjects(Guid[] ids);

	/// <summary>
	/// Runs the animation of the unit's death
	/// </summary>
	/// <param name="id"></param>
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
	/// Plays the sound
	/// </summary>
	/// <param name="id">Id of the HTML element audio</param>
	/// <param name="src">The URL of the audio to plays</param>
	/// <returns></returns>
	public Task PlaySound(string id, string src);

	/// <summary>
	/// Displays a message above the unit
	/// </summary>
	/// <param name="id">Globally unique identifier of unit</param>
	/// <param name="message"></param>
	/// <returns></returns>
	public Task ShowMessage(Guid id, string message);

	/// <summary>
	/// Rotates the unit according to the coordinates
	/// </summary>
	/// <param name="id">Globally unique identifier of unit</param>
	/// <param name="latitude"></param>
	/// <param name="longitude"></param>
	/// <returns></returns>
	public Task RotateUnit(Guid id, float latitude, float longitude);

	/// <summary>
	/// Adds a box to the map
	/// </summary>
	/// <param name="box"></param>
	/// <returns></returns>
	public Task AddBox(Box box);

	/// <summary>
	/// Converts pixel coordinates to the geographical coordinates
	/// </summary>
	/// <param name="pixelX"></param>
	/// <param name="pixelY"></param>
	/// <returns></returns>
	public Task<float[]> ConvertPixelCoordsToGlobal(double pixelX, double pixelY);

	/// <summary>
	/// Returns the geographical coordinates of the current map center.
	/// </summary>
	/// <returns></returns>
	public Task<float[]> GetCenterCoords();

	/// <summary>
	/// Gets the route on the map.
	/// </summary>
	/// <param name="startCoords"></param>
	/// <param name="endCoords"></param>
	/// <param name="routingMode"></param>
	/// <returns></returns>
	public Task<float[][]> GetRoute(float[] startCoords, float[] endCoords, string routingMode);

	/// <summary>
	/// Updates units on the map
	/// </summary>
	/// <param name="units"></param>
	/// <returns></returns>
	public Task UpdateUnits(Unit[] units);

	public Task SetPlayerManager<TValue>(DotNetObjectReference<TValue> playerManager)
		where TValue : class;

	public Task SetUnitEquipmentComponent<TValue>(DotNetObjectReference<TValue> unitEquipment)
		where TValue : class;

	public Task SetModalDialogBoxContents<TValue>(DotNetObjectReference<TValue> modalDialogBoxContents)
		where TValue : class;
}