export function addScript(src) {
    var s = document.createElement('script');
    s.setAttribute('src', src);
    document.body.appendChild(s);
}