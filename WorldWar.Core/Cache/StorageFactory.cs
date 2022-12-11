using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core.Cache
{
	internal class StorageFactory : IStorageFactory
	{
		private readonly IServiceProvider _provider;
		private readonly IDictionary<string, Type> _types;

		public StorageFactory(IServiceProvider provider, IOptions<CacheFactoryOptions> options)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
			_types = options.Value.Types;
		}

		public IStorage<T> Create<T>()
		{
			if (_types.TryGetValue(typeof(T).Name, out var type))
			{
				return (IStorage<T>)_provider.GetRequiredService(type);
			}

			throw new ArgumentOutOfRangeException(nameof(T));
		}
	}
}
