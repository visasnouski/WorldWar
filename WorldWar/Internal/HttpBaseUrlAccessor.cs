using Microsoft.Extensions.Options;
using WorldWar.YandexClient.Interfaces;
using WorldWar.YandexClient.Model;

namespace WorldWar.Internal
{
	internal class HttpBaseUrlAccessor : IHttpBaseUrlAccessor
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly YandexSettings _yandexSettings;
		public HttpBaseUrlAccessor(IHttpContextAccessor httpContextAccessor, IOptions<YandexSettings> yandexSetting)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			_yandexSettings = yandexSetting.Value ?? throw new ArgumentNullException(nameof(yandexSetting));
		}

		public Uri GetUri()
		{
			var request = _httpContextAccessor.HttpContext?.Request;
			return new Uri($"{request!.Scheme}://{request.Host}{request.PathBase}{_yandexSettings.HubConnectionUri}");
		}
	}
}
