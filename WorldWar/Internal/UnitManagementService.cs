using Microsoft.JSInterop;
using System.Collections.Concurrent;
using WorldWar.Abstractions;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Abstractions.Models.Units.Base;
using WorldWar.Exceptions;
using WorldWar.Interfaces;

namespace WorldWar.Internal;

public class UnitManagementService : IUnitManagementService
{
	// Added due to the need to manage a unit on multiple open tabs
	// The CancellationToken is used to stop all tasks of the current user.
	private static readonly ConcurrentDictionary<Guid, (CancellationTokenSource, Task)> TasksStorage = new();

	private readonly IMapStorage _mapStorage;
	private readonly IDbRepository _dbRepository;
	private readonly ICombatService _combatService;
	private readonly IMovableService _movableService;
	private readonly IInteractionObjectsService _interactionObjectsService;
	private readonly IAuthUser _authUser;

	public UnitManagementService(IMapStorage mapStorage, IDbRepository dbRepository, ICombatService combatService, IMovableService movableService, IInteractionObjectsService interactionObjectsService, IAuthUser authUser)
	{
		_mapStorage = mapStorage ?? throw new ArgumentNullException(nameof(mapStorage));
		_dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
		_combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
		_movableService = movableService ?? throw new ArgumentNullException(nameof(movableService));
		_interactionObjectsService = interactionObjectsService ?? throw new ArgumentNullException(nameof(interactionObjectsService));
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
		await StopUnit().ConfigureAwait(true);

		TasksStorage.AddOrUpdate(identity.GuidId, (_) => GetTask(cs => _movableService.StartMove(latitude, longitude, cs.Token)),
			(_, task) =>
			{
				task.Item1.Cancel(false);
				return GetTask(cs => _movableService.StartMove(latitude, longitude, cs.Token));
			});
	}

	[JSInvokable("Attack")]
	public async Task Attack(Guid enemyGuid)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);

		await StopUnit().ConfigureAwait(true);
		
		TasksStorage.AddOrUpdate(identity.GuidId, (_) => GetTask(cs => _combatService.AttackUnit(enemyGuid, cs.Token)),
			(_, task) =>
			{
				task.Item1.Cancel(false);
				return GetTask(cs => _combatService.AttackUnit(enemyGuid, cs.Token));
			});
	}

	[JSInvokable("PickUp")]
	public async Task PickUp(Guid itemGuid)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);

		await StopUnit().ConfigureAwait(true);

		TasksStorage.AddOrUpdate(identity.GuidId, (_) => GetTask(cs => _interactionObjectsService.PickUp(itemGuid, cs.Token)),
			(_, task) =>
			{
				task.Item1.Cancel(false);
				return GetTask(cs => _interactionObjectsService.PickUp(itemGuid, cs.Token));
			});
	}

	[JSInvokable("GetInCar")]
	public async Task GetInCar(Guid itemGuid)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);

		await StopUnit().ConfigureAwait(true);

		TasksStorage.AddOrUpdate(identity.GuidId, (_) => GetTask(cs => _interactionObjectsService.GetIn(itemGuid, cs.Token)),
			(_, task) =>
			{
				task.Item1.Cancel(false);
				return GetTask(cs => _interactionObjectsService.GetIn(itemGuid, cs.Token));
			});
	}

	private static (CancellationTokenSource cs, Task task) GetTask(Func<CancellationTokenSource, Task> func)
	{
		var cs = new CancellationTokenSource();
		var attackTask = Task.Run(() => func(cs), cs.Token);
		return (cs, attackTask);
	}

	[JSInvokable("StopUnit")]
	public async Task StopUnit()
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		TasksStorage.TryGetValue(identity.GuidId, out var task);
		if (task.Item1 is null)
		{
			return;
		}

		task.Item1.Cancel(false);

		try
		{
			await task.Item2.ConfigureAwait(true);
			TasksStorage.TryRemove(identity.GuidId, out _);
		}
		catch (TaskCanceledException)
		{
		}
	}
}