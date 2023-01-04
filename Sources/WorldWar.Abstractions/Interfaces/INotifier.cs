namespace WorldWar.Abstractions.Interfaces
{
	public interface INotifier
	{
		public Task SendMessage(Guid id, string message);

		public Task Die(Guid id);

		public Task Rotate(Guid id, float latitude, float longitude);

		public Task MakeNoise(string id, string src);

		public Task Attack(Guid id, float enemyLatitude, float enemyLongitude);
	}
}

	