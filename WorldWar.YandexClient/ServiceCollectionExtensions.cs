using Microsoft.Extensions.DependencyInjection;
using WorldWar.Abstractions;
using WorldWar.YandexClient.Internal;

namespace WorldWar.YandexClient
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddYandexClient(this IServiceCollection serviceCollection)
		{
			if (serviceCollection == null)
			{
				throw new ArgumentNullException(nameof(serviceCollection));
			}

			serviceCollection.AddScoped<IYandexJsClientAdapter, YandexJsClientAdapter>();
			serviceCollection.AddScoped<IYandexJsClientTransmitter, YandexJsClientTransmitter>();
			serviceCollection.AddScoped<IYandexHubConnection, YandexHubConnection>();

			return serviceCollection;
		}
	}
}