namespace WorldWar.Abstractions.Exceptions;

public class UnitNotFoundException : Exception
{
	public UnitNotFoundException(string? message)
		: base(message)
	{
	}
}