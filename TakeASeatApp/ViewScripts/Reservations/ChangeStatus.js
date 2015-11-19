$(document).on("click", "#btnConfirm", function () {
    var resId = $("#hiddenId").val();
    var status = $("#newStatus").text();
    console.log("Change status for reservation " + resId + " in " + status);
    var dataView = JSON.stringify({
        ReservationId: resId,
        NewStatus: status
    });
    $.ajax({
        url: Settings.ChangeStatusUrl,
        data: dataView,
        type: "POST",
        dataType: "html",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            console.log(RedirectUrl);
            window.location.href = RedirectUrl;
        },
        error: function (response) {
            console.log("error");
        }
    });
});