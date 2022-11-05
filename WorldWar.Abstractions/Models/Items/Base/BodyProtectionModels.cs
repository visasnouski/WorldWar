using WorldWar.Abstractions.Models.Items.Base.Protections.Body;

namespace WorldWar.Abstractions.Models.Items.Base
{
    public static class BodyProtectionModels
    {
        public static readonly BodyProtection WifeBeater = new()
        { Defense = 0, Id = 2000, ItemType = ItemTypes.BodyProtection, Name = "Wife-beater", IconPath = "protections/body/wifeBeater.png" };

        public static readonly BodyProtection Waistcoat = new()
        { Defense = 0, Id = 2001, ItemType = ItemTypes.BodyProtection, Name = "Waistcoat", IconPath = "protections/body/waistcoat.png" };
    }
}
