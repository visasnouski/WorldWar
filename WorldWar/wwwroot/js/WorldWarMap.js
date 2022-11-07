ymaps.ready(init);
var myMap;
var yandexRoute;

export function setUnitManagementService(value) {
    window.unitManagementService = value;
}
export function setUnitEquipmentComponent(value) {
    window.unitEquipmentComponent = value;
}

export function setModalDialogBoxContents(value) {
    window.modalDialogBoxContents = value;
}

export function setUserGuid(value) {
    window.userGuid = value;
}

// When click on an object on the map, a general click event is triggered, which leads to a double move.
// Ignore the global right-click event.
var ignoreRightClick = false;

function init() {
    if (myMap) {
        myMap.destroy();
        myMap = null;
        ignoreRightClick = false;
    }

    myMap = new ymaps.Map('map',
        {
            center: [53.902284, 27.561831],
            zoom: 19
        },
        {
            searchControlProvider: 'yandex#search'
        });

    myMap.events.add('contextmenu',
        function (e) {
            const coords = e.get('coords');
            if (!ignoreRightClick) {
                window.stopMotionAnimation = false;
                window.unitManagementService.invokeMethodAsync('MoveUnit', coords[0], coords[1]);
            }
            ignoreRightClick = false;
        });

    myMap.cursors.push('arrow');
}

export async function getRoute(startCoords, endCoords, routingMode) {
    let coords;

    if (yandexRoute) {
        myMap.geoObjects.remove(yandexRoute);
    }

    const multiRoute = await getYmapsRoute(startCoords, endCoords, routingMode);
    multiRoute.options.set("wayPointVisible", false);
    multiRoute.options.set("viaPointVisible", false);
    multiRoute.options.set("pinVisible", false);
    multiRoute.options.set("routeActiveMarkerVisible", false);
    yandexRoute = multiRoute.getActiveRoute();
    myMap.geoObjects.add(yandexRoute);
    const paths = yandexRoute.getPaths();

    paths.each(element => {
        coords = element.properties.get('coordinates');
    });

    return coords;
}

function getYmapsRoute(startCoords, endCoords, routingMode) {
    return ymaps.route([
        { type: 'wayPoint', point: startCoords },
        { type: 'wayPoint', point: endCoords }
    ],
        {
            multiRoute: true,
            routingMode: routingMode,
            mapStateAutoApply: false,
            wayPointVisible: false,
            viaPointVisible: false,
            pinVisible: false,
            routeActiveMarkerVisible: false,
            routeOpenBalloonOnClick: false
        });
}

export function setCoords(longitude, latitude) {
    myMap.setCenter([latitude, longitude], 19);
}

export function convertPixelCoordsToGlobal(pixelX, pixelY) {
    const projection = myMap.options.get('projection');
    return projection.fromGlobalPixels(
        myMap.converter.pageToGlobal([pixelX, pixelY]), myMap.getZoom()
    );
}

export function addUnit(unit) {
    const myGeoObject = new window.ymaps.GeoObject({
        geometry: {
            type: 'Point',
            coordinates: [unit.currentLatitude, unit.currentLongitude]
        },
        properties: {
            hintContent: unit.mobTypesString + ' ' + unit.health,
            id: unit.id,
            health: unit.health,
            unitType: unit.mobTypesString,
            weapon: unit.weapon.name,
            weaponType: unit.weapon.weaponType,
            ammo: unit.weapon.ammo,
            weaponIcon: unit.weapon.iconPath,
            magazineSize: unit.weapon.magazineSize,
            name: unit.name,
            headProtectionName: unit.headProtection.name,
            headProtectionIcon: unit.headProtection.iconPath,
            bodyProtectionName: unit.bodyProtection.name,
            bodyProtectionIcon: unit.bodyProtection.iconPath,
            rotate: unit.rotate
        }
    }, {
        zIndex: 100,
        iconLayout: createUnitLayout('<div class="sprite" style="background-position: -$[properties.spriteX]px -$[properties.spriteY]px; -webkit-transform: rotate($[properties.rotate]deg); -ms-transform: rotate($[properties.rotate]deg); transform: rotate($[properties.rotate]deg);">$[properties.aimStyle]</div>',
            function (zoom) {
                // Минимальный размер метки будет 8px, а максимальный 200px.
                // Размер метки будет расти с квадратичной зависимостью от уровня зума.
                return scale(60, zoom);
            }),
        iconImageHref: 'img/unit.png',
        iconImageSize: [28, 28],
        iconImageOffset: [-15, -16],
        iconShape: {
            type: 'Circle',
            coordinates: [0, 0],
            radius: 15
        },
        interactivityModel: 'default#geoObject',
        hintLayout: getHint()
    });

    myGeoObject.events.add('contextmenu', function (e) {
        if (window.unitManagementService) {
            const properties = e.get('target').properties;
            ignoreRightClick = true;
            window.unitManagementService.invokeMethodAsync('Attack', properties.get('id'));
        }
    });

    myGeoObject.events.add('click', function () {
        if (window.unitEquipmentComponent) {
            window.unitEquipmentComponent.invokeMethodAsync('Open', unit.id);
        }
    });

    myGeoObject.events.add('mousemove',
        function (e) {
            const properties = e.get('target').properties;
            properties.set('aimStyle', '<div class="aim-container"><div class="aim1"></div><div class="aim2"></div></div>');
        });

    myGeoObject.events.add('mouseleave',
        function (e) {
            const properties = e.get('target').properties;
            properties.set('aimStyle', '');
        });

    const positionX = getPositionX(myGeoObject);
    myGeoObject.properties.set('spriteX', positionX);
    myGeoObject.properties.set('spriteY', 0);

    myMap.geoObjects.add(myGeoObject);
}

export function addCar(car) {

    const myGeoObject = new window.ymaps.GeoObject({
        geometry: {
            type: 'Point',
            coordinates: [car.currentLatitude, car.currentLongitude]
        },
        properties: {
            hintContent: car.mobTypesString + ' ' + car.health,
            id: car.id,
            health: car.health,
            unitType: car.mobTypesString,
            weapon: car.weapon.name,
            weaponType: car.weapon.weaponType,
            ammo: car.weapon.ammo,
            weaponIcon: car.weapon.iconPath,
            magazineSize: car.weapon.magazineSize,
            name: car.name,
            headProtectionName: car.headProtection.name,
            headProtectionIcon: car.headProtection.iconPath,
            bodyProtectionName: car.bodyProtection.name,
            bodyProtectionIcon: car.bodyProtection.iconPath,
            rotate: car.rotate
        }
    },
        {
            zIndex: 1,
            iconLayout: createCarLayout(
                '<div class="car" style="-webkit-transform: rotate($[properties.rotate]deg); -ms-transform: rotate($[properties.rotate]deg); transform: rotate($[properties.rotate]deg);">$[properties.arrowStyle]</div>',
                function (zoom) {
                    const size = scale(120, zoom);
                    return size;
                }),
            interactivityModel: 'default#geoObject'
        });

    myGeoObject.events.add('contextmenu',
        function (e) {
            if (window.unitManagementService) {
                const properties = e.get('target').properties;
                ignoreRightClick = true;
                window.unitManagementService.invokeMethodAsync('GetInCar', properties.get('id'));
            }
        });

    myGeoObject.events.add('mousemove',
        function (e) {
            const properties = e.get('target').properties;
            properties.set('arrowStyle', '<div class="arrow-7"><span></span><span></span><span></span>');
        });

    myGeoObject.events.add('mouseleave',
        function (e) {
            const properties = e.get('target').properties;
            properties.set('arrowStyle', '');
        });

    myMap.geoObjects.add(myGeoObject);
}

var createUnitLayout = function (templateLayout, calculateSize) {
    // Создадим макет метки.
    var layout = ymaps.templateLayoutFactory.createClass(
        templateLayout,
        {
            build: function () {
                layout.superclass.build.call(this);
                var map = this.getData().geoObject.getMap();
                if (!this.inited) {
                    this.inited = true;
                    var zoom = map.getZoom();
                    // Подпишемся на событие изменения области просмотра карты.
                    map.events.add('boundschange', function () {
                        // Запустим перестраивание макета при изменении уровня зума.
                        const currentZoom = map.getZoom();
                        if (currentZoom != zoom) {
                            zoom = currentZoom;
                            this.rebuild();
                        }
                    }, this);
                }
                const options = this.getData().options;
                const size = calculateSize(map.getZoom());
                const element = this.getParentElement().getElementsByClassName('sprite')[0];
                const circleShape = { type: 'Circle', coordinates: [0, 0], radius: size / 4 };
                // Зададим высоту и ширину метки.
                element.style.width = element.style.height = size + 'px';

                const properties = this.getData().properties;
                const currentZoom = map.getZoom();

                const backgroundSpriteSize = scale(420, currentZoom);

                const test = backgroundSpriteSize + 'px ' + backgroundSpriteSize + 'px';

                element.style.backgroundSize = test;

                const spriteX = '-' + scale(properties.get('spriteX'), currentZoom) + 'px';
                const spriteY = '-' + scale(properties.get('spriteY'), currentZoom) + 'px';

                element.style.backgroundPositionX = spriteX;
                element.style.backgroundPositionY = spriteY;

                // Зададим смещение.
                element.style.marginLeft = element.style.marginTop = -(size / 2) + 'px';
                // Зададим фигуру активной области.
                options.set('shape', circleShape);
            }
        }
    );

    return layout;
};

function scale(size, zoom) {

    var newSize = (size - (21 - zoom) * size / 6);

    const minimalZoomLevel = 16;
    if (zoom < minimalZoomLevel) {
        newSize = 0;
    }

    return newSize;
}

var createCarLayout = function (templateLayout, calculateSize) {
    // Создадим макет метки.
    var layout = ymaps.templateLayoutFactory.createClass(
        templateLayout,
        {
            build: function () {
                layout.superclass.build.call(this);
                var map = this.getData().geoObject.getMap();
                if (!this.inited) {
                    this.inited = true;
                    var zoom = map.getZoom();
                    // Подпишемся на событие изменения области просмотра карты.
                    map.events.add('boundschange', function () {
                        // Запустим перестраивание макета при изменении уровня зума.
                        const currentZoom = map.getZoom();
                        if (currentZoom != zoom) {
                            zoom = currentZoom;
                            this.rebuild();
                        }
                    }, this);
                }
                const options = this.getData().options;
                const size = calculateSize(map.getZoom());
                const element = this.getParentElement().getElementsByClassName('car')[0];
                const circleShape = { type: 'Circle', coordinates: [0, 0], radius: size / 4 };
                // Зададим высоту и ширину метки.
                element.style.width = element.style.height = size + 'px';
                element.style.backgroundSize = size + 'px ' + size + 'px';

                // Зададим смещение.
                element.style.marginLeft = element.style.marginTop = -size / 2 + 'px';
                // Зададим фигуру активной области.
                options.set('shape', circleShape);
            }
        }
    );

    return layout;
};

export function addBox(box) {
    const myGeoObject = new window.ymaps.GeoObject({

        geometry: {
            type: 'Point',
            coordinates: [box.latitude, box.longitude]
        },
        properties: {
            id: box.id
        }
    }, {
        hintContent: 'some box',
        iconLayout: 'default#imageWithContent',
        iconImageHref: 'img/box.png',
        iconImageSize: [28, 28],
        iconImageOffset: [-14, -14]
    });

    myGeoObject.events.add('contextmenu', function (e) {
        const properties = e.get('target').properties;
        if (window.unitManagementService) {
            window.stopMotionAnimation = false;
            ignoreRightClick = true;
            window.unitManagementService.invokeMethodAsync('PickUp', properties.get('id'));
        }
    });

    myGeoObject.events.add('click', function (e) {
        const properties = e.get('target').properties;
        if (window.modalDialogBoxContents) {
            window.stopMotionAnimation = false;
            window.modalDialogBoxContents.invokeMethodAsync('Open', properties.get('id'));
        }
    });

    myMap.geoObjects.add(myGeoObject);
}

function getHint() {

    return window.ymaps.templateLayoutFactory.createClass("<div class='unit-state-hint'>" +
        '<table><tr>' +
        '<td><b>{{properties.name}}</td>' +
        '<td>({{properties.unitType}})</b></td>' +
        '</tr><tr>' +
        '<td>Health:</td>' +
        '<td>{{properties.health}}</td>' +
        '</tr><tr>' +
        '<td><img src="{{properties.weaponIcon}}" style="width:50%;"/></td>' +
        '<td>{{properties.weapon}}({{properties.ammo}}/{{properties.magazineSize}})</td>' +
        '</tr><tr>' +
        '<td><img src="{{properties.headProtectionIcon}}" style="width:50%;"/></td><td>{{properties.headProtectionName}}</td>' +
        '</tr><tr>' +
        '<td><img src="{{properties.bodyProtectionIcon}}" style="width:50%;"/></td><td>{{properties.bodyProtectionName}}</td>' +
        '</tr></table></div>', {
        getShape: function () {
            const el = this.getElement();
            var result = null;
            if (el) {
                const firstChild = el.firstChild;
                result = new window.ymaps.shape.Rectangle(
                    new window.ymaps.geometry.pixel.Rectangle([
                        [0, 0],
                        [firstChild.offsetWidth, firstChild.offsetHeight]
                    ])
                );
            }
            return result;
        }
    }
    );
}

export function updateUnit(unit) {
    myMap.geoObjects.each(function (geoObject) {
        if (geoObject.properties.get('id') === unit.id) {
            geoObject.properties.set('health', unit.health);
            geoObject.properties.set('ammo', unit.weapon.ammo);
            geoObject.properties.set('weaponIcon', unit.weapon.iconPath);
            geoObject.properties.set('weapon', unit.weapon.name);
            geoObject.properties.set('magazineSize', unit.weapon.magazineSize);
            geoObject.properties.set('headProtectionName', unit.headProtection.name);
            geoObject.properties.set('headProtectionIcon', unit.headProtection.iconPath);
            geoObject.properties.set('bodyProtectionName', unit.bodyProtection.name);
            geoObject.properties.set('bodyProtectionIcon', unit.bodyProtection.iconPath);
            geoObject.properties.set('weaponType', unit.weapon.weaponType);
            geoObject.properties.set('rotate', unit.rotate);
            const startCoords = geoObject.geometry.getCoordinates();
            const endCoords = [unit.currentLatitude, unit.currentLongitude];

            if (startCoords[1].toFixed(6) === endCoords[1].toFixed(6) &&
                startCoords[0].toFixed(6) === endCoords[0].toFixed(6)) {
                return false;
            }

            const positionX = getPositionX(geoObject);
            geoObject.properties.set('spriteX', positionX);

            const counter = 0;
            const frame = 0;
            runMotionAnimation(geoObject, startCoords[1], startCoords[0], endCoords[1], endCoords[0], counter, frame);
            // geoObject.geometry.setCoordinates(endCoords);
            return false;
        }

        return true;
    });
}

export function killUnit(id) {
    myMap.geoObjects.each(function (geoObject) {
        if (geoObject.properties.get('id') === id) {
            const positionY = 0;
            runAnimation(geoObject, positionY, 360);
            return false;
        }

        return true;
    });
}

export function shoot(id, enemyLatitude, enemyLongitude) {
    myMap.geoObjects.each(function (geoObject) {
        if (geoObject.properties.get('id') === id) {
            window.stopMotionAnimation = true;
            const positionY = 0;
            const startCoords = geoObject.geometry.getCoordinates();
            const angleDeg = -90 + degreeBearing(startCoords[0], startCoords[1], enemyLatitude, enemyLongitude);
            geoObject.properties.set('rotate', angleDeg);

            runAnimation(geoObject, positionY, getPositionX(geoObject) + 60);
            return false;
        }
        return true;
    });
}

export function showMessage(id, message) {
    myMap.geoObjects.each(function (geoObject) {
        if (geoObject.properties.get('id') === id) {
            const startCoords = geoObject.geometry.getCoordinates();

            const ballonLayout = ymaps.templateLayoutFactory.createClass(
                '<div class="slideUp">{{content}}</div>'
            );

            var balloon = new ymaps.Balloon(myMap, { closeButton: false, layout: ballonLayout });
            balloon.options.setParent(geoObject.options);
            balloon.open(startCoords, message);
            setTimeout(function () {
                balloon.close();
            }, 2000);

            return false;
        }
        return true;
    });
}

function getPositionX(geoObject) {
    const weaponType = geoObject.properties.get('weaponType');
    var positionX = 0;
    if (weaponType === 1) {
        positionX = 120;
    }
    if (weaponType === 3) {
        positionX = 240;
    }

    return positionX;
}

function runAnimation(geoObject, positionY, positionX) {
    if (positionY % 60 === 0) {
        geoObject.properties.set('spriteX', positionX);
        geoObject.properties.set('spriteY', positionY);
    }
    positionY = positionY + 10;
    if (positionY <= 360) {
        setTimeout(() => runAnimation(geoObject, positionY, positionX), 30);
    }
}

function runMotionAnimation(geoObject, x1, y1, x2, y2, percentage, frame) {
    const currentCoords = getPositionAlongTheLine(x1, y1, x2, y2, percentage);
    if (frame % 60 === 0) {

        geoObject.properties.set('spriteX', getPositionX(geoObject));
        let spriteY = geoObject.properties.get('spriteY') + 60;
        if (spriteY > 420) {
            spriteY = 0;
        }
        geoObject.properties.set('spriteY', spriteY);
    }
    geoObject.geometry.setCoordinates([currentCoords.y, currentCoords.x]);

    percentage = percentage + 0.01;
    frame = frame + 4;
    if (percentage <= 1) {
        setTimeout(() => runMotionAnimation(geoObject, x1, y1, x2, y2, percentage, frame), 10);
    }
}

function getPositionAlongTheLine(x1, y1, x2, y2, percentage) {
    return { x: x1 * (1.0 - percentage) + x2 * percentage, y: y1 * (1.0 - percentage) + y2 * percentage };
}

export function setCoordinates(id, longitude, latitude) {
    myMap.geoObjects.each(function (geoObject) {
        if (geoObject.properties.get('id') === id) {
            geoObject.geometry.setCoordinates([latitude, longitude]);
            return false;
        }
        return true;
    });
}

export function removeGeoObjects(ids) {
    ids.forEach(function (id) {
        myMap.geoObjects.each(function (geoObject) {
            if (geoObject.properties.get('id') === id) {
                myMap.geoObjects.remove(geoObject);
                return false;
            }
            return true;
        });
    });
}

export function addMob(mob) {

    const myGeoObject = new window.ymaps.GeoObject({

        geometry: {
            type: 'Point',
            coordinates: [mob.latitude, mob.longitude]
        },
        properties: {
            iconLayout: 'default#image',
            hintContent: mob.mobType + ' ' + mob.health,
            id: mob.MobGuid,
            unitType: mob.mobType,
            health: mob.health,
            weapon: mob.weapon.name,
            ammo: mob.weapon.ammo,
            weaponIcon: mob.weapon.iconPath,
            magazineSize: mob.weapon.magazineSize,
            name: mob.name,
            headProtectionName: mob.headProtection.name,
            headProtectionIcon: mob.headProtection.iconPath,
            bodyProtectionName: mob.bodyProtection.name,
            bodyProtectionIcon: mob.bodyProtection.iconPath
        }
    }, {
        draggable: true,
        iconLayout: 'default#imageWithContent',
        iconImageHref: 'img/' + mob.mobType + '.png',
        iconImageSize: [18, 18],
        iconImageOffset: [-5, -6],
        hintLayout: getHint()
    });

    myGeoObject.events.add('contextmenu', function (e) {
        ignoreRightClick = true;
        window.unitManagementService.invokeMethodAsync('Attack', e.get('target').properties.get('id'));
    });

    myMap.geoObjects.add(myGeoObject);
}

export function getCenter() {
    return myMap.getCenter();
}

export function rotateUnit(id, latitude, longitude) {
    myMap.geoObjects.each(function (geoObject) {
        if (geoObject.properties.get('id') === id) {
            const coordinates = geoObject.geometry.getCoordinates();
            const angleDeg = -90 + degreeBearing(coordinates[0], coordinates[1], latitude, longitude);
            geoObject.properties.set('rotate', angleDeg);
            return false;
        }

        return true;
    });
}

export function playAudio(elementName, src) {
    const element = document.getElementById(elementName);
    element.src = src;
    element.play();
}

function degreeBearing(lat1, lon1, lat2, lon2) {
    var dLon = toRad(lon2 - lon1);
    const dPhi = Math.log(
        Math.tan(toRad(lat2) / 2 + Math.PI / 4) / Math.tan(toRad(lat1) / 2 + Math.PI / 4));
    if (Math.abs(dLon) > Math.PI)
        dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
    return toBearing(Math.atan2(dLon, dPhi));
}

function toRad(degrees) {
    return degrees * (Math.PI / 180);
}

function toDegrees(radians) {
    return radians * 180 / Math.PI;
}

function toBearing(radians) {
    // convert radians to degrees (as bearing: 0...360)
    return (toDegrees(radians) + 360) % 360;
}

function mouseCoords(e) {
    mouseX = e.pageX;
    mouseY = e.pageY;
}