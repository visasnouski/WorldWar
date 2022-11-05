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

    public void ChangeLocation(Vector2 newPos)
    {
        _currentPos = newPos;
    }
}