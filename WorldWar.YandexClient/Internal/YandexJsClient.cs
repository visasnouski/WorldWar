using Microsoft.JSInterop;
using System.Security.Authentication;
using Microsoft.Extensions.Options;
using WorldWar.Abstractions;
using WorldWar.Abstractions.Models;

namespace WorldWar.YandexClient.Internal
{
	internal class YandexJsClient : IYandexJsClient
	{
		private readonly IJSRuntime _jsRuntime;
		private readonly ITaskDelay _taskDelay;
		private readonly IAuthUser _authUser;
		private readonly YandexSettings _yandexSettings;


		public YandexJsClient(IJSRuntime jsRuntime, ITaskDelay taskDelay, IAuthUser authUser, IOptions<YandexSettings> yandexSettings)
		{
			_jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
			_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
			_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
			_yandexSettings = yandexSettings.Value ?? throw new ArgumentNullException(nameof(yandexSettings));
		}

		public async Task<IJSObjectReference> GetYandexJsModule(string jsSrc)
		{
			var yandexMapJs = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/YandexMap.js").ConfigureAwait(true);
			var yandexSrc = $"https://api-maps.yandex.ru/2.1/?apikey={_yandexSettings.ApiKey}&lang=ru_RU";
			await yandexMapJs.InvokeVoidAsync("addScript", yandexSrc).ConfigureAwait(true);
			await _taskDelay.Delay(TimeSpan.FromMilliseconds(1000), CancellationToken.None).ConfigureAwait(true);

			var worldMapJs = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", jsSrc).ConfigureAwait(true);

			try
			{
				var authUser = await _authUser.GetIdentity().ConfigureAwait(true);
				await worldMapJs.InvokeVoidAsync("setCoords", authUser.Longitude, authUser.Latitude).ConfigureAwait(true);
				await worldMapJs.InvokeVoidAsync("setUserGuid", authUser.GuidId).ConfigureAwait(true);
			}
			catch (AuthenticationException)
			{
				await worldMapJs.InvokeVoidAsync("setCoords", 27.561831, 53.902284).ConfigureAwait(true);
			}

			return worldMapJs;
		}
	}
}
