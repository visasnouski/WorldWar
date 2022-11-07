﻿using Microsoft.Extensions.DependencyInjection;

namespace WorldWar.Core.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddStorage(this IServiceCollection serviceCollection)
		{
			if (serviceCollection == null)
			{
				throw new ArgumentNullException(nameof(serviceCollection));
			}

			serviceCollection.AddSingleton<IMapStorage, MapStorage>();
			return serviceCollection;
		}
	}
}