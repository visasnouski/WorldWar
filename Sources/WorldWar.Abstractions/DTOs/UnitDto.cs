using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorldWar.Abstractions.Models;

namespace WorldWar.Abstractions.DTOs;

[Table("Units")]
public sealed class UnitDto
{
	[Key]
	[StringLength(50)]
	public Guid Id { get; init; }

	public string Name { get; init; } = null!;

	public UnitTypes UnitType { get; init; }

	public float Longitude { get; init; }

	public float Latitude { get; init; }

	public int WeaponId { get; init; }

	[ForeignKey("WeaponId")]
	public WeaponDto Weapon { get; init; } = null!;

	public int HeadProtectionId { get; init; }

	[ForeignKey("HeadProtectionId")]
	public HeadProtectionDto HeadProtection { get; init; } = null!;

	public int BodyProtectionId { get; init; }

	[ForeignKey("BodyProtectionId")]
	public BodyProtectionDto BodyProtection { get; init; } = null!;

	public int LootId { get; set; }
	[ForeignKey("LootId")]
	public LootDto Loot { get; init; } = null!;
}