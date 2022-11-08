using Microsoft.Extensions.DependencyInjection;

namespace WorldWar.Core.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddStorage(this IServiceCollection serviceCollection)
	{
		if (serviceCollection == null)
		{
			throw new ArgumentNullException(nameof(serviceCollection));
		}

		serviceCollection.AddHostedService<DbSyncService>();
		serviceCollection.AddSingleton<IMapStorage, MapStorage>();
		serviceCollection.AddSingleton<ITasksStorage, TasksStorage>();
		return serviceCollection;
	}
}