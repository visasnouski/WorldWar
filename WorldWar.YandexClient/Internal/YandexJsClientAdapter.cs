using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.YandexClient.Interfaces;
using WorldWar.YandexClient.Model;

namespace WorldWar.YandexClient.Internal;

internal sealed class YandexJsClientAdapter : IYandexJsClientAdapter, IDisposable
{
	private readonly Lazy<Task<IJSObjectReference>> _yandexJsModule;
	private bool _isDisposed;

	public YandexJsClientAdapter(IJSRuntime jsRuntime, IAuthUser authUser, ITaskDelay taskDelay, IOptions<YandexSettings> yandexSettings)
	{
		_yandexJsModule = new Lazy<Task<IJSObjectReference>>(async () =>
			{
				var client = new YandexJsClient(jsRuntime, authUser, taskDelay, yandexSettings);
				return await client.GetYandexJsModule("./js/WorldWarMap.js").ConfigureAwait(false);
			}
		);
	}

	public async Task AddUnit(Unit unit)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);

		switch (unit.UnitType)
		{
			case UnitTypes.Car:
				await module.InvokeVoidAsync("addCar", unit).ConfigureAwait(false);
				break;
			case UnitTypes.Player or UnitTypes.Mob:
				await module.InvokeVoidAsync("addUnit", unit).ConfigureAwait(false);
				break;
		}
	}

	public async Task UpdateUnit(Unit unit)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("updateUnit", unit).ConfigureAwait(false);
	}

	public async Task RemoveGeoObjects(Guid[] ids)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("removeGeoObjects", ids).ConfigureAwait(false);
	}

	public async Task KillUnit(Guid id)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("killUnit", id).ConfigureAwait(false);
	}

	public async Task ShowMessage(Guid id, string message)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("showMessage", id, message).ConfigureAwait(false);
	}

	public async Task ShootUnit(Guid id, float enemyLatitude, float enemyLongitude)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("shoot", id, enemyLatitude, enemyLongitude).ConfigureAwait(false);
	}

	public async Task PlaySound(string elementId, string src)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("playAudio", elementId, src).ConfigureAwait(false);
	}

	public async Task RotateUnit(Guid id, float latitude, float longitude)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("rotateUnit", id, latitude, longitude).ConfigureAwait(false);
	}

	public async Task AddBox(Box box)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("addBox", box).ConfigureAwait(false);
	}

	public async Task SetUnitManagementService<TValue>(DotNetObjectReference<TValue> userManagement)
		where TValue : class
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("setUnitManagementService", userManagement).ConfigureAwait(false);
	}

	public async Task SetUnitEquipmentComponent<TValue>(DotNetObjectReference<TValue> unitEquipment)
		where TValue : class
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("setUnitEquipmentComponent", unitEquipment).ConfigureAwait(false);
	}

	public async Task SetModalDialogBoxContents<TValue>(DotNetObjectReference<TValue> modalDialogBoxContents)
		where TValue : class
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("setModalDialogBoxContents", modalDialogBoxContents).ConfigureAwait(false);
	}

	public async Task UpdateUnits(Unit[] toUpdateList)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		await module.InvokeVoidAsync("updateUnits", new object?[] { toUpdateList }).ConfigureAwait(false);
	}

	public async Task<float[]> ConvertPixelCoordsToGlobal(double pixelX, double pixelY)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		return await module.InvokeAsync<float[]>("convertPixelCoordsToGlobal", pixelX, pixelY).ConfigureAwait(false);
	}

	public async Task<float[]> GetCenterCoords()
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		return await module.InvokeAsync<float[]>("getCenter").ConfigureAwait(false);
	}

	public async Task<float[][]> GetRoute(float[] startCoords, float[] endCoords, string routingMode)
	{
		var module = await _yandexJsModule.Value.ConfigureAwait(false);
		return await module.InvokeAsync<float[][]>("getRoute", startCoords, endCoords, routingMode)
			.ConfigureAwait(false);
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
			&& _yandexJsModule.IsValueCreated
			&& _yandexJsModule.Value.IsCompleted)
		{
			_yandexJsModule.Value.Dispose();
		}

		_isDisposed = true;
	}
}