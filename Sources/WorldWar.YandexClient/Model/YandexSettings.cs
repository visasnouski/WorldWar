namespace WorldWar.YandexClient.Model;

public class YandexSettings
{
	public string? ApiKey { get; set; }

	[Obsolete("Use YandexApiSrc instead of ApiSrc")]
	public string ApiSrc { get; set; } = "https://api-maps.yandex.ru/2.1/?apikey={0}&lang=ru_RU";

#pragma warning disable CS0618 // Type or member is obsolete
	public Uri YandexApiSrc => new(string.Format(ApiSrc, ApiKey));
#pragma warning restore CS0618 // Type or member is obsolete

	public Uri? HubConnectionUri { get; set; }
}