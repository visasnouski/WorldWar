using Microsoft.JSInterop;

namespace WorldWar.YandexClient.Internal
{
    internal interface IYandexJsClient
    {
        Task<IJSObjectReference> GetYandexJsModule(string jsSrc);
    }
}