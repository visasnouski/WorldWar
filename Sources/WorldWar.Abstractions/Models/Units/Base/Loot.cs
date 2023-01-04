using WorldWar.Abstractions.Models.Items.Base;

namespace WorldWar.Abstractions.Models.Units.Base;

public sealed class Loot
{
	public int Id { get; init; }

	public ICollection<Item> Items { get; init; } = default!;
}