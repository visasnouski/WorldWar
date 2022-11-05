using System.ComponentModel.DataAnnotations.Schema;
using WorldWar.Abstractions.Models.Items.Base;

namespace WorldWar.Abstractions.DTOs
{
	[Table("Foods")]
	public class FoodDto : ItemDto
	{
		public override int Id { get; init; }

		public override string Name { get; init; } = null!;

		public override string IconPath { get; init; } = null!;

		public override ItemTypes ItemType { get; init; }

		public int Benefit { get; init; }
	}
}
