using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using WorldWar.Abstractions;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.YandexClient.Internal
{
	internal class YandexJsClientAdapter : IYandexJsClientAdapter, IDisposable
	{
		private readonly Lazy<Task<IJSObjectReference>> _yandexJsModule;
		private bool _isDisposed;

		public YandexJsClientAdapter(IJSRuntime jsRuntime, ITaskDelay taskDelay, IAuthUser authUser, IOptions<YandexSettings> yandexSettings)
		{
			_yandexJsModule = new Lazy<Task<IJSObjectReference>>(async () =>
				{
					var client = new YandexJsClient(jsRuntime, taskDelay, authUser, yandexSettings);
					var module = await client.GetYandexJsModule("./js/WorldWarMap.js").ConfigureAwait(true);
					return module;
				}
			);
		}

		public async Task AddUnit(Unit unit)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);

			switch (unit.UnitType)
			{
				case UnitTypes.Car:
					await module.InvokeVoidAsync("addCar", unit).ConfigureAwait(true);
					break;
				case UnitTypes.Player or UnitTypes.Mob:
					await module.InvokeVoidAsync("addUnit", unit).ConfigureAwait(true);
					break;
			}
		}

		public async Task UpdateUnit(Unit unit)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("updateUnit", unit).ConfigureAwait(true);
		}

		public async Task RemoveGeoObjects(Guid[] ids)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("removeGeoObjects", ids).ConfigureAwait(true);
		}

		public async Task KillUnit(Guid id)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("killUnit", id).ConfigureAwait(false);
		}

		public async Task ShowMessage(Guid id, string message)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("showMessage", id, message).ConfigureAwait(false);
		}

		public async Task ShootUnit(Guid id, float enemyLatitude, float enemyLongitude)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("shoot", id, enemyLatitude, enemyLongitude).ConfigureAwait(true);
		}

		public async Task PlaySound(string elementId, string src)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("playAudio", elementId, src).ConfigureAwait(true);
		}

		public async Task RotateUnit(Guid id, float latitude, float longitude)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("rotateUnit", id, latitude, longitude).ConfigureAwait(false);
		}

		public async Task AddBox(Box box)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("addBox", box).ConfigureAwait(true);
		}

		public async Task SetUserGuid(Guid id)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("setUserGuid", id).ConfigureAwait(true);
		}

		public async Task SetUnitManagementService(IUnitManagementService unitManagementService)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("setUnitManagementService", DotNetObjectReference.Create(unitManagementService)).ConfigureAwait(true);
		}

		public async Task SetUnitEquipmentComponent<TValue>(DotNetObjectReference<TValue> unitEquipment)
			where TValue : class
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("setUnitEquipmentComponent", unitEquipment).ConfigureAwait(true);
		}

		public async Task SetModalDialogBoxContents<TValue>(DotNetObjectReference<TValue> modalDialogBoxContents)
			where TValue : class
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			await module.InvokeVoidAsync("setModalDialogBoxContents", modalDialogBoxContents).ConfigureAwait(true);
		}

		public async Task<float[]> ConvertPixelCoordsToGlobal(double pixelX, double pixelY)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			return await module.InvokeAsync<float[]>("convertPixelCoordsToGlobal", pixelX, pixelY).ConfigureAwait(true);
		}

		public async Task<float[]> GetCenterCoords()
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			return await module.InvokeAsync<float[]>("getCenter").ConfigureAwait(true);
		}

		public async Task<float[][]> GetRoute(float[] startCoords, float[] endCoords, string routingMode)
		{
			var module = await _yandexJsModule.Value.ConfigureAwait(true);
			return await module.InvokeAsync<float[][]>("getRoute", startCoords, endCoords, routingMode)
				.ConfigureAwait(true);
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
				&& _yandexJsModule.IsValueCreated
				&& _yandexJsModule.Value.IsCompleted)
			{
				_yandexJsModule.Value.Dispose();
			}

			_isDisposed = true;
		}
	}
}
