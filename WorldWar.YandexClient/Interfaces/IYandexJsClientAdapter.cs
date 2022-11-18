﻿using Microsoft.JSInterop;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;

namespace WorldWar.YandexClient.Interfaces;

public interface IYandexJsClientAdapter
{
	public Task AddUnit(Unit unit);

	public Task UpdateUnit(Unit unit);

	public Task RemoveGeoObjects(Guid[] ids);

	public Task KillUnit(Guid id);

	public Task ShootUnit(Guid id, float enemyLatitude, float enemyLongitude);

	public Task PlaySound(string elementId, string src);

	public Task ShowMessage(Guid id, string message);

	public Task RotateUnit(Guid id, float latitude, float longitude);

	public Task AddBox(Box box);

	public Task SetUnitManagementService(IUserManagement userManagement);

	public Task SetUserGuid(Guid id);

	public Task SetUnitEquipmentComponent<TValue>(DotNetObjectReference<TValue> unitEquipment)
		where TValue : class;

	public Task SetModalDialogBoxContents<TValue>(DotNetObjectReference<TValue> modalDialogBoxContents)
		where TValue : class;

	public Task<float[]> ConvertPixelCoordsToGlobal(double pixelX, double pixelY);

	public Task<float[]> GetCenterCoords();

	public Task<float[][]> GetRoute(float[] startCoords, float[] endCoords, string routingMode);

	public Task UpdateUnits(Unit[] toUpdateList);
}