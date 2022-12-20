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
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

	public DbRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IServiceScopeFactory scopeFactory)
	{
		_dbContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
		_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
	}

	public IReadOnlyCollection<Weapon> Weapons
	{
		get
		{
			using var applicationDbContext = _dbContextFactory.CreateDbContext();
			return applicationDbContext.Weapons.Select(x => x.ToWeapon()).ToList();
		}
	}

	public IReadOnlyCollection<Unit> Units
	{
		get
		{
			using var scope = _scopeFactory.CreateScope();
			using var applicationDbContext = _dbContextFactory.CreateDbContext();
			var unitFactory = scope.ServiceProvider.GetRequiredService<IUnitFactory>();

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
		}
	}

	public IReadOnlyCollection<BodyProtection> BodyProtections
	{
		get
		{
			using var scope = _scopeFactory.CreateScope();
			var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			return applicationDbContext.BodyProtections.Select(x => x.ToBodyProtection()).ToList();
		}
	}

	public IReadOnlyCollection<HeadProtection> HeadProtections
	{
		get
		{
			using var applicationDbContext = _dbContextFactory.CreateDbContext();
			return applicationDbContext.HeadProtections.Select(x => x.ToHeadProtection()).ToList();
		}
	}

	public async Task<Weapon> GetWeapon(int id)
	{
		await using var applicationDbContext = await _dbContextFactory.CreateDbContextAsync();
		var weaponDto = await applicationDbContext.Weapons.FirstOrDefaultAsync(x => x.Id == id);

		if (weaponDto == null)
		{
			throw new ItemNotFoundException($"Weapon with id {id} not found");
		}

		return weaponDto.ToWeapon();
	}

	public async Task<BodyProtection> GetBodyProtection(int id)
	{
		await using var applicationDbContext = await _dbContextFactory.CreateDbContextAsync();
		var bodyProtectionDto = await applicationDbContext.BodyProtections.FirstOrDefaultAsync(x => x.Id == id);

		if (bodyProtectionDto == null)
		{
			throw new ItemNotFoundException($"BodyProtection with id {id} not found");
		}

		return bodyProtectionDto.ToBodyProtection();
	}

	public async Task<HeadProtection> GetHeadProtection(int id)
	{
		await using var applicationDbContext = await _dbContextFactory.CreateDbContextAsync();
		var headProtectionDto = await applicationDbContext.HeadProtections.FirstOrDefaultAsync(x => x.Id == id);

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

		var unitDto = await applicationDbContext.Units
				.Include(x => x.Weapon)
				.Include(x => x.BodyProtection)
				.Include(x => x.HeadProtection)
				.Include(x => x.Loot)
				.FirstOrDefaultAsync(x => x.Id == id);

		if (unitDto == null)
		{
			throw new UnitNotFoundException($"Unit with id {id} not found");
		}

		return unitDto.ToUnit(unitFactory, itemIds => itemIds.Select(itemId => applicationDbContext.Items.First(x => itemId == x.Id)).ToArray());
	}

	public async Task SetUnit(Unit unit)
	{
		var unitDto = unit.ToUnitDto();

		await using var applicationDbContext = await _dbContextFactory.CreateDbContextAsync();

		applicationDbContext.HeadProtections.Attach(unitDto.HeadProtection);
		applicationDbContext.BodyProtections.Attach(unitDto.BodyProtection);
		applicationDbContext.Weapons.Attach(unitDto.Weapon);

		await applicationDbContext.Units.AddAsync(unitDto);
		await applicationDbContext.SaveChangesAsync();

	}

	public async Task UpdateUnit(Unit unit)
	{
		var unitDto = unit.ToUnitDto();

		await using var applicationDbContext = await _dbContextFactory.CreateDbContextAsync();

		applicationDbContext.Weapons.Attach(unitDto.Weapon);
		applicationDbContext.BodyProtections.Attach(unitDto.BodyProtection);
		applicationDbContext.HeadProtections.Attach(unitDto.HeadProtection);

		if (applicationDbContext.Units.AsNoTracking().Any(x => x.Id == unitDto.Id))
		{
			applicationDbContext.Units.Update(unitDto);
		}
		else
		{
			await applicationDbContext.Units.AddAsync(unitDto);
		}

		await applicationDbContext.SaveChangesAsync();
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

			if (unit is Car car && car.TryGetOffWheel(out var driver))
			{
				// TODO Refactor 
				await UpdateUnit(driver!);
			}

			await UpdateUnit(unit);
		}
	}
}
