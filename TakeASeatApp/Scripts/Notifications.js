$(document).ready(function () {
    $("#notificationsCounter").hide();
    getNotifications();
    setInterval(function () {
        getNotifications();
    }, 30 * 1000);
});
function getNotifications() {
    $.ajax({
        url: Settings.NotificationUrl,
        type: "GET",
        dataType: "html",
        success: function (response) {
            console.log(response)
            if (response != '0') {
                $("#notificationsCounter").html(response);
                $("#notificationsCounter").show();
            }
            else {
                $("#notificationsCounter").hide();
            }
        },
        error: function (response) {
            console.log("Error in retrieving notifications.");
        }
    });
}