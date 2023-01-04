using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorldWar.Abstractions.Models.Items.Base;

namespace WorldWar.Abstractions.DTOs;

[Table("Items")]
public abstract class ItemDto
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public abstract int Id { get; init; }

	public abstract ItemTypes ItemType { get; init; }

	public abstract string Name { get; init; }

	public abstract string IconPath { get; init; }

}