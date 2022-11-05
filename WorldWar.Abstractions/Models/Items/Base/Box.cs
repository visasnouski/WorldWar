namespace WorldWar.Abstractions.Models.Items.Base
{
    public class Box
    {
        public float Latitude { get; init; }

        public float Longitude { get; init; }

        public ICollection<Item> Items { get; init; }

        public Guid Id { get; init; }

        public Box(Guid id, float latitude, float longitude, ICollection<Item> items)
        {
            Id = id;
            Latitude = latitude;
            Longitude = longitude;
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }
    }
}
