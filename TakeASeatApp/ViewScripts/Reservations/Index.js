var map;
var infoWindow;
var request;
var geocoder;
var markers = [];
var pos;

function initialize() {
    var mapOptions = {
        zoom: 15
    };
    map = new google.maps.Map(document.getElementById('map-canvas'),
        mapOptions);
    geocoder = new google.maps.Geocoder();

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            pos = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
            map.setCenter(pos);
            var self = "/TakeASeat/Content/Images/Self.png";
            var crrLocMarker = new google.maps.Marker({
                position: pos,
                map: map,
                icon: self
            });
            google.maps.event.addListener(crrLocMarker, 'click', function () {
                infoWindow.setContent("You are here!");
                infoWindow.open(map, this);
            });
        }, function () {
            handleNoGeolocation(true);
        });
    }
    else {
        //Browser doesn't support geolocation
        handleNoGeolocation(false);
    }
}

function getAddressFromLatlng(latlng) {
    geocoder.geocode({ 'latLng': latlng }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            if (results[1]) {
                $("#inputAddress").val(results[1].formatted_address);
                return results[1].formatted_address;
            } else {
                console.log('No results found');
            }
        } else {
            console.log('Geocoder failed due to: ' + status);
        }
    });
}

function handleNoGeolocation(errorFlag) {
    var content;
    if (errorFlag) {
        content = "Error: The Geolocation service has failed.";
    }
    else {
        content = "Error: Your browser does not support geolocation.";
    }
    var options = {
        map: map,
        position: new google.maps.LatLng(60, 105),
        content: content
    };
    infoWindow = new google.maps.InfoWindow(options);
    map.setCenter(options.position);
}

function createMarker(posToAdd) {
    var markerToAdd = new google.maps.Marker({
        position: posToAdd,
        map: map
    });
    var crrInfowindow = new google.maps.InfoWindow({
        content: "Place"
    });
    google.maps.event.addListener(markerToAdd, 'click', function () {
        crrInfowindow.open(map, this);
    });
    //var marker = new google.maps.Marker({
    //    map: map,
    //    position: placeLoc
    //});
    //google.maps.event.addListener(marker, 'click', function () {
    //    infoWindow.setContent(place.name);
    //    infoWindow.open(map, this);
    //});
    markers.push(markerToAdd);
}

// Deletes all markers in the array by removing references to them.
function deleteMarkers() {
    clearMarkers();
    markers = [];
}

// Removes the markers from the map, but keeps them in the array.
function clearMarkers() {
    setAllMap(null);
}

// Sets the map on all markers in the array.
function setAllMap(map) {
    for (var i = 0; i < markers.length; i++) {
        markers[i].setMap(map);
        //console.log(markers[i]);
    }
}

$(document).ready(function () {
    initialize();
    $(".location input").each(function () {
        var lat_lng = $(this).val();
        var latitude = lat_lng.substring(0, lat_lng.indexOf('+'));
        var longitude = lat_lng.substring(lat_lng.indexOf('+') + 1, lat_lng.length);
        var posToAdd = new google.maps.LatLng(latitude, longitude);
        createMarker(posToAdd);
        //console.log(lat_lng);
    });
    setAllMap(map);
});

$(document).on("click", "tr.restaurantEntry .location", function () {
    var input = $(this)[0].children[0];
    var lat_lng = $(input).val();
    var latitude = lat_lng.substring(0, lat_lng.indexOf('+'));
    var longitude = lat_lng.substring(lat_lng.indexOf('+') + 1, lat_lng.length);
    var posToCentrate = new google.maps.LatLng(latitude, longitude);
    map.panTo(posToCentrate);
    map.setZoom(18);
});

$(document).on("click", ".btnDeleteReservation", function () {
    var resId = this.id.substring(this.id.indexOf("-") + 1, this.id.length);
    console.log("Confirm delete reservation " + resId);
    $.ajax({
        url: "Reservation/Delete",
        data: { id: resId },
        type: "POST",
        dataType: "html",
        success: function (response) {
            $("#" + resId).modal("hide");
            $("body").removeClass("modal-open");
            $(".modal-backdrop").remove();
            //$("#reservationsList").html(response);
            $("#icon-" + resId).remove();
            $("#" + resId).remove();
        },
        error: function (response) {
            alert("error");
        }
    });
});

//MANAGERS
$(document).on("click", "#managerActions button", function () {
    var resId = this.id.substring(this.id.indexOf("-") + 1, this.id.length);
    var status = this.id.substring(0, this.id.indexOf("-"));
    console.log("Change status for reservation " + resId + " in " + status);
    var dataView = JSON.stringify({
        ReservationId: resId,
        NewStatus: status
    });
    $(this).parent().html("New status: " + status);
    $.ajax({
        url: "Reservation/ChangeStatus",
        data: dataView,
        type: "POST",
        dataType: "html",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            //$("#reservationsList").html(response);
            $("#status-" + resId).text(status);
        },
        error: function (response) {
            alert("error");
        }
    });
});