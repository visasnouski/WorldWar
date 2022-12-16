using Microsoft.Extensions.Options;
using WorldWar.YandexClient.Interfaces;
using WorldWar.YandexClient.Model;

namespace WorldWar.Internal
{
	internal class HttpBaseUrlAccessor : IHttpBaseUrlAccessor
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly YandexSettings _yandexSettings;
		private Uri? _baseUri;

		public HttpBaseUrlAccessor(IHttpContextAccessor httpContextAccessor, IOptions<YandexSettings> yandexSetting)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			_yandexSettings = yandexSetting.Value ?? throw new ArgumentNullException(nameof(yandexSetting));
		}

		public Uri GetUri()
		{
			return _baseUri ??= GetBaseUri();
		}

		private Uri GetBaseUri()
		{
			var request = _httpContextAccessor.HttpContext?.Request;
			return new Uri($"{request?.Scheme}://{request?.Host}{request?.PathBase}{_yandexSettings.HubConnectionUri}");
		}
	}
}
