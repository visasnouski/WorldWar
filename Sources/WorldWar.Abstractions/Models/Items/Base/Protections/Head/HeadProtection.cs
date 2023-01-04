namespace WorldWar.Abstractions.Models.Items.Base.Protections.Head;

public class HeadProtection : Protection
{
	public override int Id { get; init; }

	public override string Name { get; init; } = null!;

	public override ItemTypes ItemType { get; init; }

	public override int Defense { get; init; }

	public override string IconPath { get; init; } = null!;

	public override int Size => 1;
}