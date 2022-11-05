using WorldWar.Abstractions.Models.Items.Base;

namespace WorldWar.Abstractions.Models.Units.Base
{
    public class Loot
    {
        public int Id { get; init; }

        public virtual IList<Item> Items { get; init; } = default!;
    }
}
