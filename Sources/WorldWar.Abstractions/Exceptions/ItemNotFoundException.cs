namespace WorldWar.Abstractions.Exceptions;

public class ItemNotFoundException : Exception
{
	public ItemNotFoundException(string? message)
		: base(message)
	{
	}
}