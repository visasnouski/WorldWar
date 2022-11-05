namespace WorldWar.Abstractions.Models.Items.Base.Protections;

public abstract class Protection : Item
{
    public abstract int Defense { get; init; }
}