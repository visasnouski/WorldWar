using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorldWar.Abstractions.Models.Items.Base;

namespace WorldWar.Abstractions.DTOs
{
    public abstract class ProtectionDto : ItemDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override int Id { get; init; }

        public override string Name { get; init; } = null!;

        public override ItemTypes ItemType { get; init; }

        public override string IconPath { get; init; } = null!;

        public int Defense { get; init; }
    }
}
