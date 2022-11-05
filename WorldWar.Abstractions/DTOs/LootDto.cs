using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorldWar.Abstractions.DTOs
{
    [Table("Loots")]
    public class LootDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; init; }

        public ICollection<int> ItemIds { get; init; } = default!;
    }
}
