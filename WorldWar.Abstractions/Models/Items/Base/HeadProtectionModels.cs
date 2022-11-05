using WorldWar.Abstractions.Models.Items.Base.Protections.Head;

namespace WorldWar.Abstractions.Models.Items.Base
{
    public static class HeadProtectionModels
    {
        public static readonly HeadProtection Bandana = new()
        { Defense = 0, Id = 3000, ItemType = ItemTypes.HeadProtection, Name = "Bandana", IconPath = "protections/head/bandana.png" };

        public static readonly HeadProtection Cap = new()
        { Defense = 0, Id = 3001, ItemType = ItemTypes.HeadProtection, Name = "Cap", IconPath = "protections/head/cap.png" };
    }
}
