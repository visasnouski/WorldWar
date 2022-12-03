using Microsoft.JSInterop;
using System.Security.Authentication;
using Microsoft.Extensions.Options;
using WorldWar.Abstractions.Interfaces;
using WorldWar.YandexClient.Model;
using WorldWar.YandexClient.Interfaces;

namespace WorldWar.YandexClient.Internal;

internal class YandexJsClient : IYandexJsClient
{
	private readonly IJSRuntime _jsRuntime;
	private readonly IAuthUser _authUser;
	private readonly ITaskDelay _taskDelay;
	private readonly YandexSettings _yandexSettings;

	public YandexJsClient(IJSRuntime jsRuntime, IAuthUser authUser, ITaskDelay taskDelay, IOptions<YandexSettings> yandexSettings)
	{
		_jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
		_authUser = authUser ?? throw new ArgumentNullException(nameof(authUser));
		_taskDelay = taskDelay ?? throw new ArgumentNullException(nameof(taskDelay));
		_yandexSettings = yandexSettings.Value ?? throw new ArgumentNullException(nameof(yandexSettings));
	}

	public async Task<IJSObjectReference> GetYandexJsModule(string jsSrc)
	{
		var yandexMapJs = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/YandexMap.js").ConfigureAwait(false);
		await yandexMapJs.InvokeVoidAsync("addScript", _yandexSettings.YandexApiSrc).ConfigureAwait(false);

		// TODO Find a way to await for the initialization of the Yandex map in javascript
		await _taskDelay.Delay(TimeSpan.FromSeconds(1), CancellationToken.None).ConfigureAwait(false);

		var worldMapJs = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", jsSrc).ConfigureAwait(false);

		try
		{
			var authUser = await _authUser.GetIdentity().ConfigureAwait(false);
			await worldMapJs.InvokeVoidAsync("setCoords", authUser.Longitude, authUser.Latitude).ConfigureAwait(false);
		}
		catch (AuthenticationException)
		{
			await worldMapJs.InvokeVoidAsync("setCoords", 27.561831, 53.902284).ConfigureAwait(false);
		}

		return worldMapJs;
	}
}