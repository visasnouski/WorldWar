namespace WorldWar.Abstractions.Models.Units.Base;

public interface IMovable
{
	public float Speed { get; }

	public Location Location { get; init; }

	public double Rotate { get; }

	public void Move(TimeSpan time, float endLongitude, float endLatitude, int acceleration = 1);

	public void RotateUnit(float endLongitude, float endLatitude, float? fromLongitude = null, float? fromLatitude = null);

	public void SaveCurrentLocation();

	public bool IsWithinReach(float endLongitude, float endLatitude, float? weaponDistance = null);
}