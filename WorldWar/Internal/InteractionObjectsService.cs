using WorldWar.Abstractions.Exceptions;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Components.States;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.Interfaces;

namespace WorldWar.Internal;

internal class InteractionObjectsService : IInteractionObjectsService
{
	private readonly IStorage<Unit> _unitsStorage;
	private readonly IStorage<Box> _boxStorage;
	private readonly IMovableService _movableService;
	private readonly IAuthUser _authUser;
	private readonly InteractStates _interactStates;

	public InteractionObjectsService(IStorageFactory storageFactory, IMovableService movableService, IAuthUser authUser, InteractStates interactStates)
	{
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_boxStorage = storageFactory.Create<Box>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
		_interactStates = interactStates ?? throw new ArgumentNullException(nameof(interactStates));
	}

	public async Task PickUp(Guid guidId, bool isUnit, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);

		if (!_unitsStorage.TryGetValue(identity.GuidId, out var user))
		{
			return;
		}

		var coords = GetCoordinates(isUnit, guidId, _unitsStorage, _boxStorage);

		if (!user!.IsWithinReach(coords.longitude, coords.latitude))
		{
			float[][] route = {
				new[] { coords.latitude,coords.longitude }
			};

			await _movableService.StartMove(user!.Id, route, cancellationToken).ConfigureAwait(true);
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

	public async Task GetIn(Guid guidId, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		if (!_unitsStorage.TryGetValue(identity.GuidId, out var user))
		{
			return;
		}

		if (user!.Id == guidId)
		{
			await GetOut(guidId, cancellationToken).ConfigureAwait(true);
			return;
		}

		if (!_unitsStorage.TryGetValue(guidId, out var unit))
		{
			return;
		}

		if (!user.IsWithinReach(unit!.Longitude, unit.Latitude))
		{
			float[][] route = {
				new[] { unit.Latitude, unit.Longitude }
			};

			await _movableService.StartMove(user.Id, route, cancellationToken).ConfigureAwait(true);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		if (unit is Car)
		{
			user.ChangeUnitType(UnitTypes.Car);
			_unitsStorage.AddOrUpdate(user.Id, user);
			_unitsStorage.Remove(unit.Id);
		}
	}

	public async Task GetOut(Guid guidId, CancellationToken cancellationToken)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		if (!_unitsStorage.TryGetValue(identity.GuidId, out var user))
		{
			return;
		}

		user!.ChangeUnitType(UnitTypes.Player);
		var id = Guid.NewGuid();
		_unitsStorage.AddOrUpdate(id, new Car(id, GenerateName.Generate(7), user!.Latitude, user.Longitude, 100));
		_unitsStorage.AddOrUpdate(user.Id, user);
	}

	private static (float latitude, float longitude) GetCoordinates(bool isUnit, Guid id, IStorage<Unit> unitStorage, IStorage<Box> boxStorage)
	{
		if (isUnit && unitStorage.TryGetValue(id, out var unit))
		{
			return (unit!.Latitude, unit.Longitude);
		}

		if (boxStorage.TryGetValue(id, out var box))
		{
			return (box!.Latitude, box.Longitude);
		}

		throw new ItemNotFoundException($"Item {id} not found.");
	}
}

