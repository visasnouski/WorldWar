using Microsoft.JSInterop;
using WorldWar.Abstractions.Exceptions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Abstractions.Models.Units.Base;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;
using WorldWar.Repository.interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Internal;

internal class PlayerManager : IPlayerManager
{
	private readonly IStorage<Unit> _unitsStorage;
	private readonly IDbRepository _dbRepository;
	private readonly IUnitManagementService _unitManagementService;
	private readonly IYandexJsClientAdapter _yandexJsClientAdapter;
	private readonly IAuthUser _authUser;
	private readonly IUnitFactory _unitFactory;
	private readonly ILogger<PlayerManager> _logger;

	public PlayerManager(IStorageFactory storageFactory, IDbRepository dbRepository, IUnitManagementService unitManagementService, IAuthUser authUser, IYandexJsClientAdapter yandexJsClientAdapter, IUnitFactory unitFactory, ILogger<PlayerManager> logger)
	{
		_unitsStorage = storageFactory.Create<Unit>() ?? throw new ArgumentNullException(nameof(storageFactory));
		_dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
		_unitManagementService = unitManagementService ?? throw new ArgumentNullException(nameof(unitManagementService));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
		_yandexJsClientAdapter = yandexJsClientAdapter ?? throw new ArgumentNullException(nameof(yandexJsClientAdapter));
		_unitFactory = unitFactory ?? throw new ArgumentNullException(nameof(unitFactory));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task AddUnit()
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		try
		{
			var unit = await _dbRepository.GetUnit(identity.GuidId).ConfigureAwait(true);
			_unitsStorage.AddOrUpdate(unit.Id, unit);
		}
		catch (UnitNotFoundException)
		{
			var unit = _unitFactory.Create(UnitTypes.Player,
				identity.GuidId,
				identity.UserName,
				(float)identity.Latitude,
				(float)identity.Longitude,
				100,
				loot: new Loot()
				{
					Id = identity.GuidId.GetHashCode(),
					Items = new List<Item>(),
				});

			await _dbRepository.SetUnit(unit).ConfigureAwait(true);
			_unitsStorage.AddOrUpdate(unit.Id, unit);
		}
	}

	[JSInvokable("MoveUnit")]
	public async Task MoveUnit(float latitude, float longitude)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);
		if (!_unitsStorage.TryGetValue(identity.GuidId, out var unit))
		{
			return;
		}

		var startCoords = new[] { unit!.Latitude, unit.Longitude };
		var endCoords = new[] { latitude, longitude };

		var routingMode = unit.UnitType == UnitTypes.Car ? "auto" : "pedestrian";

		var route = await _yandexJsClientAdapter.GetRoute(startCoords, endCoords, routingMode);
		await _unitManagementService.MoveUnit(unit, route).ConfigureAwait(true);
	}

	[JSInvokable("Attack")]
	public async Task Attack(Guid enemyGuid)
	{
		var identity = await _authUser.GetIdentity().ConfigureAwait(true);

		if (!_unitsStorage.TryGetValue(identity.GuidId, out var user))
		{
			_logger.LogWarning("The user {guid} not found.", identity.GuidId);
			return;
		}

		if (!_unitsStorage.TryGetValue(enemyGuid, out var enemy))
		{
			_logger.LogWarning("The enemy {guid} not found.", enemyGuid);
			return;
		}

		await _unitManagementService.Attack(user!, enemy!).ConfigureAwait(true);
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