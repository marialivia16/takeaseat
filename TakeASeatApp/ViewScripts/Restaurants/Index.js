function getAddressFromLatlng(latlng) {
    var geocoder = new google.maps.Geocoder();
    geocoder.geocode({ 'latLng': latlng }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            if (results[0]) {
                $("#address").text(results[0].formatted_address);
                console.log(results[0].formatted_address);
            } else {
                console.log('No results found');
            }
        } else {
            console.log('Geocoder failed due to: ' + status);
        }
    });
}

$(window).bind("load", function () {
    var lat = $("#Restaurant_Latitude").val();
    var lng = $("#Restaurant_Longitude").val();
    console.log(lat + " " + lng);
    getAddressFromLatlng(new google.maps.LatLng(lat, lng));
});