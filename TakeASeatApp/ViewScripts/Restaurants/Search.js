var inputAddress;
var map;
var infoWindow;
var request;
var geocoder;
var markers = [];
var pos;
var input = (document.getElementById("inputAddress"));
var searchBox = new google.maps.places.SearchBox((input));

$(document).ready(function () {
    initialize();
    $(".resultBlock input").each(function () {
        var lat_lng = $(this).val();
        var latitude = lat_lng.substring(0, lat_lng.indexOf('+'));
        var longitude = lat_lng.substring(lat_lng.indexOf('+') + 1, lat_lng.length);
        var posToAdd = new google.maps.LatLng(latitude, longitude);
        createMarker(posToAdd);
        console.log($(this).parent())
    });
    setAllMap(map);
});

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
            $("#inputAddress").val(getAddressFromLatlng(pos));
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
            if (results[0]) {
                $("#inputAddress").val(results[0].formatted_address);
                return results[0].formatted_address;
            } else {
                console.log('No results found');
            }
        } else {
            console.log('Geocoder failed due to: ' + status);
        }
    });
}

function getAddressFromLatlngForResults(lat, lng, id) {
    var latlng = new google.maps.LatLng(lat, lng);
    geocoder.geocode({ 'latLng': latlng }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            if (results[0]) {
                $(id).text(results[0].formatted_address);
                console.log("Results from latlng: " + results[0].formatted_address);
            } else {
                console.log('No results found');
            }
        } else {
            console.log('No results found');
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



$(document).on("click", ".centerMap", function () {
    var parent = $(this).parent().parent().parent().parent();
    var lat_lng = $(parent[0].children[2]).val();
    var latitude = lat_lng.substring(0, lat_lng.indexOf('+'));
    var longitude = lat_lng.substring(lat_lng.indexOf('+') + 1, lat_lng.length);
    console.log(latitude + " " + longitude);
    var posToCentrate = new google.maps.LatLng(latitude, longitude);
    map.panTo(posToCentrate);
    map.setZoom(18);
});

$("#btnSearchRestaurants").click(function () {
    deleteMarkers();
    $("#loadingContainer img").show();
    //if ($("#inputAddress").val() === "") $("#inputAddress").val(getAddressFromLatlng(pos));
    inputAddress = $("#inputAddress").val();
    console.log(inputAddress);
    //deleteMarkers();
    geocoder.geocode({ 'address': inputAddress }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            map.setCenter(results[0].geometry.location);
            //createMarker(results[0]);
            var tagsToAdd = [];
            $("#tagsList span").each(function () {
                tagsToAdd.push($(this).text());
            });
            //tagsToAdd = tagsToAdd.length > 0 ? tagsToAdd : null;
            var distance = $("#inputDistance").val();
            var dataView = JSON.stringify({
                Latitude: results[0].geometry.location.lat(),
                Longitude: results[0].geometry.location.lng(),
                Distance: distance,
                TagsToAdd: tagsToAdd
            });
            $.ajax({
                url: "SearchResults",
                data: dataView,
                type: "POST",
                //traditional: true,
                dataType: "html",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    $("#loadingContainer img").hide();
                    $("#searchResults").html(data);
                    $("#searchResults").show();
                    $("#searchOptions").slideToggle();
                    $("#anotherSearch").show();
                    var date = $("#Date").val();
                    $("#modalReservation .modalDate").val(date);
                    var time = $("#Time").val();
                    $("#modalReservation .modalTime").val(time);
                    var persons = $("#inputPersons").val();
                    $("#modalReservation .modalPersons").val(persons);
                    var duration = $("#inputDuration").val();
                    $("#modalReservation .modalDuration").val(duration);
                    //map
                    $(".resultBlock input").each(function () {
                        var lat_lng = $(this).val();
                        var latitude = lat_lng.substring(0, lat_lng.indexOf('+'));
                        var longitude = lat_lng.substring(lat_lng.indexOf('+') + 1, lat_lng.length);
                        var posToAdd = new google.maps.LatLng(latitude, longitude);
                        createMarker(posToAdd);
                        console.log($(this).parent())
                    });
                    setAllMap(map);
                },
                error: function (data) {
                    console.log("Error in Ajax");
                }
            });
        }
        else {
            //error message
        }
    });
});

$(document).on("click", "#getCurrentAddress", function () {
    $("#inputAddress").val(getAddressFromLatlng(pos));
});

$(document).on("click", "#tagsListModal span", function () {
    var tagId = "#" + this.id;
    if ($(tagId).hasClass("btn-default")) { //add to list
        $(tagId).removeClass("btn-default").addClass("btn-info");
        var appendSpan = "<span class=\"btn btn-info btn-sm\" style=\"margin-right: 5px; margin-bottom: 5px;\" id=" + tagId.substring(tagId.lastIndexOf("-") + 1, tagId.length) + ">" + this.innerText + "</span>";
        $("#tagsList").append(appendSpan);
    }
    else if ($(tagId).hasClass("btn-info")) { //remove from list
        var IdToRemove = tagId.substring(tagId.lastIndexOf("-") + 1, tagId.length);
        $(tagId).removeClass("btn-info").addClass("btn-default");
        $("#tagsList #" + IdToRemove).remove();
    }
});

$(document).on("click", "#tagsList span", function () {
    $("#tagsListModal #tag-" + this.id).removeClass("btn-info").addClass("btn-default");
    $("#" + this.id).remove();
});

$(document).on("click", "#anotherSearch", function () {
    $("#anotherSearch").hide();
    $("#searchOptions").slideToggle();
    $("#searchResults").slideToggle();
});

//$(document).on('click', '.result', function () {
//    //$("#restaurantDetails").val(this.id);
//});

$(document).ready(function () {
    $("#anotherSearch").hide();
    $("#loadingContainer img").hide();
});

//MAKE RESERVATION

$(document).on("click", ".btnMakeReservation", function () {
    var restId = this.id.substring(this.id.indexOf("-") + 1, this.id.length);
    $("#loadingContainer-" + restId + " img").show();
    var dateId = "#DateModal-" + restId;
    var timeId = "#TimeModal-" + restId;
    var personsId = "#PersonsModal-" + restId;
    var durationId = "#DurationModal-" + restId;
    var date = $(dateId).val();
    var time = $(timeId).val();
    var persons = $(personsId).val();
    var duration = $(durationId).val() * 60;

    console.log(date + " " + time);
    var datetime = new Date(date + " " + time);
    console.log(datetime);

    var dataView = JSON.stringify({
        RestaurantId: restId,
        Duration: duration,
        NumberOfGuests: persons,
        DateAndTime: datetime.toISOString()
    });
    console.log(dataView);

    $.ajax({
        url: SearchSettings.ReservationUrl,
        data: dataView,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response) {
            //alert("success");
            $("#reservationConfirmation-" + restId).html(response);
            $("#loadingContainer-" + restId + " img").hide();
        },
        error: function (response) {
            alert("error");
        }
    });
});
