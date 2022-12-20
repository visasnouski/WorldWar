using WorldWar.Abstractions.Extensions;
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
	private readonly IMovableService _movableService;
	private readonly InteractStates _interactStates;

	public InteractionObjectsService(IStorageFactory storageFactory, IMovableService movableService, InteractStates interactStates)
	{
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_interactStates = interactStates ?? throw new ArgumentNullException(nameof(interactStates));
	}

	public async Task PickUp(Unit unit, Box targetItem, CancellationToken cancellationToken)
	{
		if (!unit.IsWithinReach(targetItem.Longitude, targetItem.Latitude))
		{
			float[][] route = {
				new[] { targetItem.Latitude, targetItem.Longitude }
			};

			await _movableService.StartMove(unit, route, cancellationToken);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		_interactStates.Show(targetItem.Id);
	}

	public async Task PickUp(Unit unit, Unit targetUnit, CancellationToken cancellationToken)
	{
		if (!unit.IsWithinReach(targetUnit.Longitude, targetUnit.Latitude))
		{
			float[][] route = {
				new[] { targetUnit.Latitude, targetUnit.Longitude }
			};

			await _movableService.StartMove(unit, route, cancellationToken);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		_interactStates.Show(targetUnit.Id, true);
	}

	public async Task GetIn(Unit unit, Unit targetUnit, CancellationToken cancellationToken)
	{
		if (!unit.IsWithinReach(targetUnit.Longitude, targetUnit.Latitude))
		{
			float[][] route = { new[] { targetUnit.Latitude, targetUnit.Longitude } };
			await _movableService.StartMove(unit, route, cancellationToken);

			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		if (targetUnit is Car car)
		{
			_unitsStorage.Remove(car.Id);
			car.GetBehindWheel(unit);
			_unitsStorage.AddOrUpdate(car.Id, car);
		}
	}

	public Task GetOut(Unit targetUnit, CancellationToken cancellationToken)
	{
		if (targetUnit is Car car && car.TryGetOffWheel(out var unit))
		{
			_unitsStorage.AddOrUpdate(car.Id, car);
			_unitsStorage.AddOrUpdate(unit!.Id, unit);
		}

		return Task.CompletedTask;
	}
}

