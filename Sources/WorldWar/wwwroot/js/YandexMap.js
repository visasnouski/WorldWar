export function addScript(src) {
    var yandexScript = document.createElement('script');
    yandexScript.setAttribute('src', src);
    document.body.appendChild(yandexScript);
}
