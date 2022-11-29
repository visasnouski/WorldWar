using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WorldWar.Abstractions.Models.Units;
using WorldWar.Core.Interfaces;

namespace WorldWar.Core.Cache
{
	public class CacheFactory : ICacheFactory
	{
		private readonly IServiceProvider _provider;
		private readonly IDictionary<string, Type> _types;

		public CacheFactory(IServiceProvider provider, IOptions<CacheFactoryOptions> options)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
			_types = options.Value.Types;
		}
		public IStorage<T> Create<T>()
		where T : IStorable
		{
			if (_types.TryGetValue(typeof(T).Name, out var type))
			{
				return (IStorage<T>)_provider.GetRequiredService(type);
			}

			throw new ArgumentOutOfRangeException(nameof(T));
		}
	}
}
