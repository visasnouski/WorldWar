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
	private readonly IMovableService _movableService;
	private readonly InteractStates _interactStates;
	private readonly IUnitFactory _unitFactory;

	public InteractionObjectsService(IStorageFactory storageFactory, IMovableService movableService, InteractStates interactStates, IUnitFactory unitFactory)
	{
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_interactStates = interactStates ?? throw new ArgumentNullException(nameof(interactStates));
		_unitFactory = unitFactory ?? throw new ArgumentNullException(nameof(unitFactory));
	}

	public async Task PickUp(Unit unit, Box targetItem, CancellationToken cancellationToken)
	{
		if (unit.IsWithinReach(targetItem.Longitude, targetItem.Latitude))
		{
			float[][] route = {
				new[] { targetItem.Latitude, targetItem.Longitude }
			};

			await _movableService.StartMove(unit, route, cancellationToken).ConfigureAwait(true);
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

			await _movableService.StartMove(unit, route, cancellationToken).ConfigureAwait(true);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		_interactStates.Show(targetUnit.Id, true);
	}

	public async Task GetIn(Unit unit, Unit targetUnit, CancellationToken cancellationToken)
	{
		if (unit.Id == targetUnit.Id)
		{
			await GetOut(unit, targetUnit, cancellationToken).ConfigureAwait(true);
			return;
		}

		if (!unit.IsWithinReach(targetUnit.Longitude, targetUnit.Latitude))
		{
			float[][] route = { new[] { targetUnit.Latitude, targetUnit.Longitude } };
			await _movableService.StartMove(unit, route, cancellationToken).ConfigureAwait(true);

			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}

		if (targetUnit is Car)
		{
			unit.ChangeUnitType(UnitTypes.Car);
			_unitsStorage.AddOrUpdate(unit.Id, unit);
			_unitsStorage.Remove(targetUnit.Id);
		}
	}

	public Task GetOut(Unit unit, Unit targetUnit, CancellationToken cancellationToken)
	{
		unit.ChangeUnitType(UnitTypes.Player);
		var id = Guid.NewGuid();
		_unitsStorage.AddOrUpdate(id, _unitFactory.Create(UnitTypes.Car, id, GenerateName.Generate(7), unit.Latitude, unit.Longitude, 100));
		_unitsStorage.AddOrUpdate(unit.Id, unit);

		return Task.CompletedTask;
	}
}

