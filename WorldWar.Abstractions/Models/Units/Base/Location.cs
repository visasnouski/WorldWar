using System.Numerics;

namespace WorldWar.Abstractions.Models.Units.Base;

public class Location
{
	private Vector2? _currentPos;
	private Vector2 _startPos;

	public Vector2 CurrentPos => _currentPos ?? _startPos;
	public Vector2 StartPos => _startPos;

	public Location(float longitude, float latitude)
	{
		_startPos = new Vector2(longitude, latitude);
	}

	public void SaveCurrentLocation()
	{
		if (_currentPos.HasValue)
		{
			_startPos = _currentPos.Value;
		}
	}

	public float GetDistance(Location targetLocation)
	{
		return Vector2.Distance(CurrentPos, targetLocation.CurrentPos);
	}

	public void Move(TimeSpan time, float endLongitude, float endLatitude, float speed)
	{
		var endPos = new Vector2(endLongitude, endLatitude);
		var movVec = Vector2.Subtract(endPos, StartPos);
		var normMovVec = Vector2.Normalize(movVec);

		// When the waypoint matches the unit's location
		if (normMovVec.X is Single.NaN || normMovVec.Y is Single.NaN)
		{
			return;
		}

		var deltaVec = normMovVec * Convert.ToInt64(time.TotalSeconds) * speed;
		_currentPos = Vector2.Add(StartPos, deltaVec);
	}
}