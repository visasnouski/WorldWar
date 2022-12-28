using Microsoft.Extensions.DependencyInjection;
using WorldWar.Abstractions.Interfaces;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.Core
{
	internal class Notifier : INotifier
	{
		private readonly IYandexJsClientNotifier _yandexJsClientNotifier;

		public Notifier(IServiceScopeFactory scopeFactory)
		{
			_yandexJsClientNotifier =
				scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IYandexJsClientNotifier>() ??
				throw new ArgumentNullException(nameof(scopeFactory));
		}

		public Task Attack(Guid id, float enemyLatitude, float enemyLongitude)
		{
			return _yandexJsClientNotifier.AttackUnit(id, enemyLatitude, enemyLongitude);
		}

		public Task Die(Guid id)
		{
			return _yandexJsClientNotifier.KillUnit(id);
		}

		public Task MakeNoise(string id, string src)
		{
			return _yandexJsClientNotifier.PlaySound(id, src);
		}

		public Task Rotate(Guid id, float latitude, float longitude)
		{
			return _yandexJsClientNotifier.RotateUnit(id, latitude, longitude);
		}

		public Task SendMessage(Guid id, string message)
		{
			return _yandexJsClientNotifier.SendMessage(id, message);
		}
	}
}
