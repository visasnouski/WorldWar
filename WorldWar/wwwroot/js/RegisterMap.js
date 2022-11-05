ymaps.ready(init);
var registerMap;

export function init() {
    registerMap = new ymaps.Map("registerMap", {
        center: [53.902284, 27.561831],
        zoom: 15
    }, {
        searchControlProvider: 'yandex#search'
    });

    registerMap.cursors.push('arrow');
}

export function setCoords(longitude, latitude) {
    registerMap.setCenter([latitude, longitude], 15);
}

export function getCenter() {
    return registerMap.getCenter();
}