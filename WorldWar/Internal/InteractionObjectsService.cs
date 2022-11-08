﻿using WorldWar.Abstractions;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Components.States;
using WorldWar.Core;
using WorldWar.Interfaces;

namespace WorldWar.Internal;

public class InteractionObjectsService : IInteractionObjectsService
{
	private readonly IMovableService _movableService;
	private readonly IMapStorage _mapStorage;
	private readonly IAuthUser _authUser;
	private readonly InteractStates _interactStates;

	public InteractionObjectsService(IMovableService movableService, IMapStorage mapStorage, IAuthUser authUser, InteractStates interactStates)
	{
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
		_interactStates = interactStates ?? throw new ArgumentNullException(nameof(interactStates));
	}

	public async Task PickUp(Guid guidId, bool isUnit, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		var user = await _mapStorage.GetUnit(identity.GuidId).ConfigureAwait(true);
		var coords = await GetCoordinates(isUnit, guidId, _mapStorage).ConfigureAwait(true);

		if (!user.IsWithinReach(coords.longitude, coords.latitude))
		{
			await _movableService.StartMoveAlongRoute(user.Id, coords.latitude, coords.longitude, cancellationToken).ConfigureAwait(true);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		if (isUnit)
		{
			_interactStates.Show(guidId, true);
		}


		_interactStates.Show(guidId);
	}

	private static async Task<(float latitude, float longitude)> GetCoordinates(bool isUnit, Guid id, IMapStorage mapStorage)
	{
		if (isUnit)
		{
			var unit = await mapStorage.GetUnit(id).ConfigureAwait(true);
			return (unit.CurrentLatitude, unit.CurrentLongitude);
		}

		var box = await mapStorage.GetItem(id).ConfigureAwait(true);
		return (box.Latitude, box.Longitude);
	}

	public async Task GetIn(Guid guidId, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		var user = await _mapStorage.GetUnit(identity.GuidId).ConfigureAwait(true);
		if (user.Id == guidId)
		{
			await GetOut(guidId, cancellationToken).ConfigureAwait(true);
			return;
		}
		var unit = await _mapStorage.GetUnit(guidId).ConfigureAwait(true);

		if (!user.IsWithinReach(unit.CurrentLongitude, unit.CurrentLatitude))
		{
			await _movableService.StartMoveAlongRoute(user.Id, unit.CurrentLatitude, unit.CurrentLongitude, cancellationToken).ConfigureAwait(true);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		if (unit is Car)
		{
			user.ChangeUnitType(UnitTypes.Car);
			await _mapStorage.SetUnit(user).ConfigureAwait(true);
			await _mapStorage.RemoveUnit(unit).ConfigureAwait(true);
		}
	}

	public async Task GetOut(Guid guidId, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		var user = await _mapStorage.GetUnit(identity.GuidId).ConfigureAwait(true);
		user.ChangeUnitType(UnitTypes.Player);

		await _mapStorage.SetUnit(new Car(Guid.NewGuid(), GenerateName.Generate(7), user.CurrentLatitude, user.CurrentLongitude, 100)).ConfigureAwait(true);
		await _mapStorage.SetUnit(user).ConfigureAwait(true);
	}
}

