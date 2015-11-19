$(document).on('click', '#tagsListModal span', function () {
    var tagId = "#" + this.id;
    if ($(tagId).hasClass("btn-default")) { //add to list
        $(tagId).removeClass("btn-default").addClass("btn-info");
        var appendSpan = '<span class="btn btn-info btn-sm" style="margin-right: 5px; margin-bottom: 5px;" id=' + tagId.substring(tagId.lastIndexOf("-") + 1, tagId.length) + '>' + this.textContent + '</span>';
        $("#tagsList").append(appendSpan);
    }
    else if ($(tagId).hasClass("btn-info")) { //remove from list
        var IdToRemove = tagId.substring(tagId.lastIndexOf("-") + 1, tagId.length);
        $(tagId).removeClass("btn-info").addClass("btn-default");
        $('#tagsList #' + IdToRemove).remove();
    }
});

$(document).on('click', '#tagsList span', function () {
    $("#tagsListModal #tag-" + this.id).removeClass("btn-info").addClass("btn-default");
    $('#' + this.id).remove();
});


$("#btnEditRestaurant").click(function () {
    var tagsToAdd = [];
    $("#tagsList span").each(function () {
        tagsToAdd.push($(this).text());
    });

    var id = $("#Id").val();
    var name = $("#Name").text();
    var description = $("#Description").val();
    var phone = $("#PhoneNumber").val();
    var web = $("#WebAddress").val();
    console.log(tagsToAdd);
    console.log(id + " " + name + " " + description + " " + phone);

    var dataView = JSON.stringify({
        Id: id,
        Name: name,
        Description: description,
        PhoneNumber: phone,
        WebAddress: web,
        TagsToAdd: tagsToAdd
    });

    $.ajax({
        url: "Edit",
        data: dataView,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response) {
            //alert("success");
            window.location.href = "Index";
        },
        error: function (response) {
            console.log("Error " + response);
        }
    });
});

$("#newTagInput").keyup(function (e) {
    if (e.keyCode == 13) {
        var appendSpan = '<span class="btn btn-info btn-sm" style="margin-right: 5px; margin-bottom: 5px;">' + $("#newTagInput").val() + '</span>';
        $("#tagsList").append(appendSpan);
        $("#newTagInput").val("");
    }
});

//image upload
$(document).on('click', '#imageUpload', function () {
    $("#addImage").click();
});

$(document).on('change', '#addImage', function () {
    console.log(this.files[0].size / 1024 / 1024);
    var size = this.files[0].size / 1024 / 1024;
    if (size > 2) alert("Only submit images that are smaller than 2MB!");
    else {
        $("progressbar").show();
        var files = this.files;
        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                for (var x = 0; x < files.length; x++) {
                    data.append("file" + x, files[x]);
                }
                var id = $("#Id").val();
                var name = files[0].name;
                $.ajax({
                    type: "POST",
                    url: '/Restaurants/UploadImage?id=' + id,
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (result) {
                        console.log(result);
                        $("#imageUpload").attr("src", "/Content/Images/RestaurantImage/" + id + ".jpg?random=" + new Date().getTime());
                        $("progressbar").hide();
                    },
                    error: function (xhr, status, p3, p4) {
                        var err = "Error " + " " + status + " " + p3 + " " + p4;
                        if (xhr.responseText && xhr.responseText[0] == "{")
                            err = JSON.parse(xhr.responseText).Message;
                        console.log(err);
                    }
                });
            } else {
                alert("This browser doesn't support HTML5 file uploads!");
            }
        }
    }
});

function progressHandlingFunction(e) {
    if (e.lengthComputable) {
        $('progress').attr({ value: e.loaded, max: e.total });
    }
}