using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Items.Base.Weapons;

namespace WorldWar.Abstractions.DTOs
{
	[Table("Weapons")]
	public class WeaponDto : ItemDto
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public override int Id { get; init; }

		public override ItemTypes ItemType { get; init; }

		public override string Name { get; init; } = null!;

		public WeaponTypes WeaponType { get; init; }

		public float Distance { get; init; }

		public int Damage { get; init; }

		public int MagazineSize { get; init; }

		public int Accuracy { get; init; }

		public override string IconPath { get; init; } = null!;

		public TimeSpan DelayShot { get; init; }

		public string ShotSoundLocation { get; init; } = null!;

		public string ReloadSoundLocation { get; init; } = null!;

		public TimeSpan ReloadTime { get; init; }
	}
}
