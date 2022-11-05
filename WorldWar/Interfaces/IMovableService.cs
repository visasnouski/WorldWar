namespace WorldWar.Interfaces
{
	public interface IMovableService
	{
		public Task StartMove(float latitude, float longitude, CancellationToken cancellationToken, float? weaponDistance = null);

		public Task StartMove(Guid targetGuid, CancellationToken cancellationToken, float? distance = null);

		public Task Rotate(Guid id, float latitude, float longitude, CancellationToken cancellationToken);
	}
}