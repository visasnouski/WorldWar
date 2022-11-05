namespace WorldWar.Exceptions
{
	public class UnitNotFoundException : Exception
	{
		public UnitNotFoundException(string? message)
			: base(message)
		{
		}
	}
}
