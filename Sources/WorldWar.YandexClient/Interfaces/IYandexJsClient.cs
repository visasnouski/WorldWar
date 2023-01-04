using Microsoft.JSInterop;

namespace WorldWar.YandexClient.Interfaces;

internal interface IYandexJsClient
{
	Task<IJSObjectReference> GetYandexJsModule(string jsSrc);
}