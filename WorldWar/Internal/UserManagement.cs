﻿using Microsoft.JSInterop;
using WorldWar.Abstractions;
using WorldWar.Abstractions.Exceptions;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Abstractions.Models.Units.Base;
using WorldWar.Core;
using WorldWar.Repository.interfaces;

namespace WorldWar.Internal;

public class UserManagement : IUserManagement
{
	private readonly IMapStorage _mapStorage;
	private readonly IDbRepository _dbRepository;
	private readonly IUnitManagementService _unitManagementService;
	private readonly IAuthUser _authUser;

	public UserManagement(IMapStorage mapStorage, IDbRepository dbRepository, IUnitManagementService unitManagementService, IAuthUser authUser)
	{
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
		_unitManagementService = unitManagementService ?? throw new ArgumentNullException(nameof(unitManagementService));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
	}

	public async Task AddUnit()
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		try
		{
			var unit = await _dbRepository.GetUnit(identity.GuidId).ConfigureAwait(true);
			await _mapStorage.SetUnit(unit).ConfigureAwait(true);
		}
		catch (UnitNotFoundException)
		{
			var unit = new Player(
				identity.GuidId,
				identity.UserName,
				(float)identity.Latitude,
				(float)identity.Longitude,
				100,
				loot: new Loot()
				{
					Id = identity.GuidId.GetHashCode(),
					Items = new List<Item>() { WeaponModels.TT, WeaponModels.Ak47, WeaponModels.DesertEagle, BodyProtectionModels.Waistcoat, HeadProtectionModels.Cap, WeaponModels.TT, WeaponModels.DesertEagle, WeaponModels.DesertEagle, WeaponModels.DesertEagle, WeaponModels.DesertEagle, WeaponModels.TT, WeaponModels.TT, WeaponModels.TT, WeaponModels.TT, WeaponModels.TT, WeaponModels.TT, WeaponModels.TT, WeaponModels.TT, },
				});

			await _dbRepository.SetUnit(unit).ConfigureAwait(true);
			await _mapStorage.SetUnit(unit).ConfigureAwait(true);
		}
	}

	[JSInvokable("MoveUnit")]
	public async Task MoveUnit(float latitude, float longitude)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		await _unitManagementService.MoveUnit(identity.GuidId, latitude, longitude, true).ConfigureAwait(true);
	}

	[JSInvokable("Attack")]
	public async Task Attack(Guid enemyGuid)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		await _unitManagementService.Attack(identity.GuidId, enemyGuid).ConfigureAwait(true);
	}

	[JSInvokable("PickUp")]
	public async Task PickUp(Guid itemGuid, bool isUnit)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		await _unitManagementService.PickUp(identity.GuidId, itemGuid, isUnit).ConfigureAwait(true);
	}

	[JSInvokable("GetInCar")]
	public async Task GetInCar(Guid itemGuid)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		await _unitManagementService.GetInCar(identity.GuidId, itemGuid).ConfigureAwait(true);
	}

	[JSInvokable("StopUnit")]
	public async Task StopUnit()
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		await _unitManagementService.StopUnit(identity.GuidId).ConfigureAwait(true);
	}
}