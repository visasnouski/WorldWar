using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using WorldWar.Abstractions.DTOs;
using WorldWar.Abstractions.Extensions;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Repository.Models;

namespace WorldWar.Repository.Internal;

public class ApplicationDbContext : IdentityDbContext<MyIdentityUser>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<UnitDto> Units { get; set; }

	public DbSet<WeaponDto> Weapons { get; set; } = null!;

	public DbSet<FoodDto> Foods { get; set; } = null!;

	public DbSet<ItemDto> Items { get; set; } = null!;

	public DbSet<LootDto> Loots { get; set; } = null!;

	public DbSet<BodyProtectionDto> BodyProtections { get; set; } = null!;

	public DbSet<HeadProtectionDto> HeadProtections { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		builder.Entity<WeaponDto>().HasData(
			WeaponModels.TT.ToWeaponDto(),
			WeaponModels.Fist.ToWeaponDto(),
			WeaponModels.DesertEagle.ToWeaponDto(),
			WeaponModels.Ak47.ToWeaponDto()
		);

		builder.Entity<BodyProtectionDto>().HasData(
			BodyProtectionModels.WifeBeater.ToBodyProtectionDto(),
			BodyProtectionModels.Waistcoat.ToBodyProtectionDto()
		);

		builder.Entity<HeadProtectionDto>().HasData(
			HeadProtectionModels.Bandana.ToHeadProtectionDto(),
			HeadProtectionModels.Cap.ToHeadProtectionDto()
			);

		var valueComparer = new ValueComparer<ICollection<int>>(
			(c1, c2) => c1!.SequenceEqual(c2!),
			c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
			c => c.ToHashSet());

		builder.Entity<LootDto>().Property(p => p.ItemIds)
			.HasConversion(v => JsonConvert.SerializeObject(v),
			 v => JsonConvert.DeserializeObject<List<int>>(v)!)
			.Metadata.SetValueComparer(valueComparer);

		base.OnModelCreating(builder);
	}
}