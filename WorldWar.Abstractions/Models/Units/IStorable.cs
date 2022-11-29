namespace WorldWar.Abstractions.Models.Units
{
	public interface IStorable
	{
		public Guid Id { get; init; }

		public float Latitude { get; }

		public float Longitude { get; }
	}
}