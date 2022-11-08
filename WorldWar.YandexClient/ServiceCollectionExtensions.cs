using Microsoft.Extensions.DependencyInjection;
using WorldWar.YandexClient.Interfaces;
using WorldWar.YandexClient.Internal;
using WorldWar.YandexClient.Model;

namespace WorldWar.YandexClient
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddYandexClient(this IServiceCollection serviceCollection, Action<YandexSettings> configureOptions)
		{
			if (serviceCollection == null)
			{
				throw new ArgumentNullException(nameof(serviceCollection));
			}

			serviceCollection.Configure(configureOptions);
			serviceCollection.AddScoped<IYandexJsClientAdapter, YandexJsClientAdapter>();
			serviceCollection.AddScoped<IYandexJsClientTransmitter, YandexJsClientTransmitter>();
			serviceCollection.AddScoped<IYandexHubConnection, YandexHubConnection>();

			return serviceCollection;
		}
	}
}