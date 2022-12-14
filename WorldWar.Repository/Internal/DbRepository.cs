using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldWar.Abstractions.Exceptions;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base.Protections.Body;
using WorldWar.Abstractions.Models.Items.Base.Protections.Head;
using WorldWar.Abstractions.Models.Items.Base.Weapons;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Repository.interfaces;

namespace WorldWar.Repository.Internal;

internal class DbRepository : IDbRepository
{
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private readonly IServiceScopeFactory _scopeFactory;

	public DbRepository(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
	}

	public IReadOnlyCollection<Weapon> Weapons
	{
		get
		{
			using var scope = _scopeFactory.CreateScope();
			var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			return Lock(() => applicationDbContext.Weapons.Select(x => x.ToWeapon()).ToList());
		}
	}

	public IReadOnlyCollection<Unit> Units
	{
		get
		{
			using var scope = _scopeFactory.CreateScope();
			var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var unitFactory = scope.ServiceProvider.GetRequiredService<IUnitFactory>();
			return Lock(() =>
			{
				var unitDtos = applicationDbContext.Units
					.Include(x => x.Weapon)
					.Include(x => x.BodyProtection)
					.Include(x => x.HeadProtection)
					.Include(x => x.Loot).ToArray();

				var items = applicationDbContext.Items.ToArray();

				return unitDtos.Select(x => x.ToUnit(unitFactory, itemIds =>
						itemIds.Select(itemId => items.First(item => itemId == item.Id))
							.ToArray()))
					.ToArray();
			});
		}
	}

	public IReadOnlyCollection<BodyProtection> BodyProtections
	{
		get
		{
			using var scope = _scopeFactory.CreateScope();
			var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			return Lock(() => applicationDbContext.BodyProtections.Select(x => x.ToBodyProtection()).ToList());
		}
	}
	public IReadOnlyCollection<HeadProtection> HeadProtections
	{
		get
		{
			using var scope = _scopeFactory.CreateScope();
			var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			return Lock(() => applicationDbContext.HeadProtections.Select(x => x.ToHeadProtection()).ToList());
		}
	}

	public async Task<Weapon> GetWeapon(int id)
	{
		using var scope = _scopeFactory.CreateScope();
		var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var weaponDto = await LockAsync(() => applicationDbContext.Weapons.FirstOrDefaultAsync(x => x.Id == id))
			.ConfigureAwait(true);

		if (weaponDto == null)
		{
			throw new ItemNotFoundException($"Weapon with id {id} not found");
		}

		return weaponDto.ToWeapon();
	}

	public async Task<BodyProtection> GetBodyProtection(int id)
	{
		using var scope = _scopeFactory.CreateScope();
		var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var bodyProtectionDto = await LockAsync(() => applicationDbContext.BodyProtections.FirstOrDefaultAsync(x => x.Id == id))
			.ConfigureAwait(true);

		if (bodyProtectionDto == null)
		{
			throw new ItemNotFoundException($"BodyProtection with id {id} not found");
		}

		return bodyProtectionDto.ToBodyProtection();
	}

	public async Task<HeadProtection> GetHeadProtection(int id)
	{
		using var scope = _scopeFactory.CreateScope();
		var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var headProtectionDto = await LockAsync(() => applicationDbContext.HeadProtections.FirstOrDefaultAsync(x => x.Id == id))
			.ConfigureAwait(true);

		if (headProtectionDto == null)
		{
			throw new ItemNotFoundException($"BodyProtection with id {id} not found");
		}

		return headProtectionDto.ToHeadProtection();
	}

	public async Task<Unit> GetUnit(Guid id)
	{
		using var scope = _scopeFactory.CreateScope();
		var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var unitFactory = scope.ServiceProvider.GetRequiredService<IUnitFactory>();

		var unitDto = await LockAsync(() => applicationDbContext.Units
				.Include(x => x.Weapon)
				.Include(x => x.BodyProtection)
				.Include(x => x.HeadProtection)
				.Include(x => x.Loot)
				.FirstOrDefaultAsync(x => x.Id == id))
			.ConfigureAwait(true);

		if (unitDto == null)
		{
			throw new UnitNotFoundException($"Unit with id {id} not found");
		}

		return unitDto.ToUnit(unitFactory, itemIds => itemIds.Select(itemId => applicationDbContext.Items.First(x => itemId == x.Id)).ToArray());
	}

	public async Task SetUnit(Unit unit)
	{
		var unitDto = unit.ToUnitDto();

		using var scope = _scopeFactory.CreateScope();
		var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		Lock(() =>
		{
			applicationDbContext.HeadProtections.Attach(unitDto.HeadProtection);
			applicationDbContext.BodyProtections.Attach(unitDto.BodyProtection);
			return applicationDbContext.Weapons.Attach(unitDto.Weapon);
		});

		await LockAsync(async () =>
		{
			await applicationDbContext.Units.AddAsync(unitDto).ConfigureAwait(true);
			return await applicationDbContext.SaveChangesAsync().ConfigureAwait(true);
		}).ConfigureAwait(true);
	}

	public async Task UpdateUnit(Unit unit)
	{
		var unitDto = unit.ToUnitDto();

		using var scope = _scopeFactory.CreateScope();
		var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		Lock(() =>
		{
			applicationDbContext.Weapons.Attach(unitDto.Weapon);
			applicationDbContext.BodyProtections.Attach(unitDto.BodyProtection);
			return applicationDbContext.HeadProtections.Attach(unitDto.HeadProtection);
		});

		if (Lock(() => applicationDbContext.Units.AsNoTracking().Any(x => x.Id == unitDto.Id)))
		{
			Lock(() => applicationDbContext.Units.Update(unitDto));
		}
		else
		{
			await LockAsync(async () => await applicationDbContext.Units.AddAsync(unitDto).ConfigureAwait(true)).ConfigureAwait(true);
		}

		await LockAsync(async () => await applicationDbContext.SaveChangesAsync().ConfigureAwait(true)).ConfigureAwait(true);
	}

	public async Task SetUnits(IEnumerable<Unit> units, CancellationToken cancellationToken)
	{
		if (units == null)
		{
			throw new ArgumentNullException(nameof(units));
		}

		foreach (var unit in units)
		{
			cancellationToken.ThrowIfCancellationRequested();

			await UpdateUnit(unit).ConfigureAwait(true);
		}
	}

	private async Task<T> LockAsync<T>(Func<Task<T>> worker)
	{
		await _semaphore.WaitAsync().ConfigureAwait(true);
		try
		{
			return await worker().ConfigureAwait(true);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	private T Lock<T>(Func<T> worker)
	{
		_semaphore.Wait();
		try
		{
			return worker();
		}
		finally
		{
			_semaphore.Release();
		}
	}
}
