using WorldWar.Abstractions.Models.Units.Base;

namespace WorldWar.Abstractions.Models.Items.Base;

public class Box
{
	public float Latitude { get; init; }

	public float Longitude { get; init; }

	public Loot Loot { get; init; }

	public Guid Id { get; init; }

	public double Rotate { get; }

	public Box(Guid id, float latitude, float longitude, ICollection<Item> items, double rotate = 0)
	{
		Id = id;
		Latitude = latitude;
		Longitude = longitude;
		Loot = new Loot() { Id = id.GetHashCode(), Items = items };
		Rotate = rotate;
	}
}