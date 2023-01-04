namespace WorldWar.Abstractions.Models.Items.Base;

public abstract class Item
{
    public abstract int Id { get; init; }

    public abstract string Name { get; init; }

    public abstract ItemTypes ItemType { get; init; }

    public abstract string IconPath { get; init; }

    public abstract int Size { get; }
}