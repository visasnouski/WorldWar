using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldWar.Repository.interfaces;
using WorldWar.Repository.Internal;

namespace WorldWar.Repository;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDbRepository(this IServiceCollection serviceCollection, string connectionString, IdentityBuilder identityBuilder)
	{
		if (serviceCollection == null)
		{
			throw new ArgumentNullException(nameof(serviceCollection));
		}

		// Add services to the container.
		serviceCollection.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(connectionString));
		serviceCollection.AddDatabaseDeveloperPageExceptionFilter();

		identityBuilder.AddEntityFrameworkStores<ApplicationDbContext>();
		serviceCollection.AddSingleton<IDbRepository, DbRepository>();

		return serviceCollection;
	}
}