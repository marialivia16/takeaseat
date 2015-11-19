var map;
var infoWindow;
var request;
var geocoder;
var markers = [];
var pos;
var selectedPlace;
function initialize() {
    var mapOptions = {
        zoom: 18
    };
    var map = new google.maps.Map(document.getElementById('map-canvas'), mapOptions);
    geocoder = new google.maps.Geocoder();
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            pos = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
            map.setCenter(pos);
            var self = "/TakeASeat/Content/Images/Self.png";
            var crrLocMarker = new google.maps.Marker({
                position: pos,
                map: map,
                icon: self,
                title: "Current position"
            });
            var crrInfowindow = new google.maps.InfoWindow({
                content: "You are here!"
            });
            google.maps.event.addListener(crrLocMarker, 'click', function () {
                crrInfowindow.open(map, this);
            });
        }, function () {
            handleNoGeolocation(true);
        });
    }
    else {
        //Browser doesn't support geolocation
        handleNoGeolocation(false);
    }

    // Create the search box and link it to the UI element.
    var input = (document.getElementById('pac-input'));

    var searchBox = new google.maps.places.SearchBox((input));

    // Listen for the event fired when the user selects an item from the
    // pick list. Retrieve the matching places for that item.
    google.maps.event.addListener(searchBox, 'places_changed', function () {
        var places = searchBox.getPlaces();

        if (places.length == 0) {
            return;
        }
        for (var i = 0, marker; marker = markers[i]; i++) {
            marker.setMap(null);
        }

        // For each place, get the icon, place name, and location.
        markers = [];
        for (var i = 0, place; place = places[i]; i++) {
            var image = {
                url: place.icon,
                size: new google.maps.Size(71, 71),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(17, 34),
                scaledSize: new google.maps.Size(25, 25)
            };

            // Create a marker for each place.
            var marker = new google.maps.Marker({
                map: map,
                icon: image,
                title: place.name,
                position: place.geometry.location
            });

            markers.push(marker);
            selectedPlace = place;
            map.setCenter(selectedPlace.geometry.location);
            var infoWindow = new google.maps.InfoWindow({
                content: "<h3>" + selectedPlace.name + "</h3>"
            });
            google.maps.event.addListener(marker, 'click', function () {
                infoWindow.open(map, this);
            });
        }
    });
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

google.maps.event.addDomListener(window, 'load', initialize);

$("#btnAddRestaurant").click(function () {
    dbLat = selectedPlace.geometry.location.lat();
    dbLng = selectedPlace.geometry.location.lng();
    console.log(selectedPlace.place_id + ", " + selectedPlace.name + ", " + dbLat + ", " + dbLng);
    var dataView = JSON.stringify({
        Id: selectedPlace.place_id,
        Name: selectedPlace.name,
        Latitude: dbLat,
        Longitude: dbLng
    });
    $.ajax({
        url: "AddRestaurant",
        data: dataView,
        type: "POST",
        dataType: "html",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            //$("#addResult").html(data);
            //$("#btnAddRestaurant").hide();
            //$("#pac-input").hide();
            window.location.href = "Index";
        },
        error: function(data) {
            console.log("Error in add restaurant");
        }
    });
});