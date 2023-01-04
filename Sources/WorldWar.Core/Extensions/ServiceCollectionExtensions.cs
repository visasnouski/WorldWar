using Microsoft.Extensions.DependencyInjection;
using WorldWar.Abstractions.Interfaces;
using WorldWar.Abstractions.Models.Items.Base;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.BackgroundServices;
using WorldWar.Core.Cache;
using WorldWar.Core.Interfaces;

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
		serviceCollection.AddSingleton<IStorageFactory, StorageFactory>();
		serviceCollection.AddSingleton<Storage<Unit>>();
		serviceCollection.AddSingleton<Storage<Box>>();
		serviceCollection.Configure<CacheFactoryOptions>(options => options.Register<Unit>());
		serviceCollection.Configure<CacheFactoryOptions>(options => options.Register<Box>());

		serviceCollection.AddSingleton<ITasksStorage, TasksStorage>();
		return serviceCollection;
	}

	public static IServiceCollection AddUnitServices(this IServiceCollection serviceCollection)
	{
		if (serviceCollection == null)
		{
			throw new ArgumentNullException(nameof(serviceCollection));
		}

		serviceCollection.AddSingleton<IUnitFactory, UnitFactory>();
		serviceCollection.AddSingleton<INotifier, Notifier>();

		serviceCollection.AddScoped<ICombatService, CombatService>();
		serviceCollection.AddScoped<IMovableService, MovableService>();
		return serviceCollection;
	}
}