using Microsoft.JSInterop;

namespace WorldWar.Abstractions
{
    public interface IYandexJsClient
    {
        Task<IJSObjectReference> GetYandexJsModule(string jsSrc);
    }
}