using Microsoft.Extensions.DependencyInjection;

namespace WorldWar.AI;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAiService(this IServiceCollection serviceCollection)
	{
		if (serviceCollection == null)
		{
			throw new ArgumentNullException(nameof(serviceCollection));
		}

		serviceCollection.AddSingleton<AiService>();
		serviceCollection.AddHostedService(provider => provider.GetService<AiService>());

		return serviceCollection;
	}
}