namespace WorldWar.YandexClient.Interfaces
{
	public interface IHttpBaseUrlAccessor
	{
		/// <summary>
		/// Gets the base URL of the application
		/// </summary>
		/// <returns>The instance of <see cref="Uri"/></returns>
		public Uri GetUri();
	}
}