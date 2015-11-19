var MyPage = {};

function btnStart_Click()
{
	var showProcess = function()
		{
			$("#TsqlFiles").slideDown(600, ProcessNextScript);
		};
	$("#Intro").slideUp(600, showProcess);
}

function ProcessNextScript()
{
	MyPage.FilesList.eq(MyPage.CurrentFileIndex).show();
	MyPage.ScriptFileNameCtl.val(MyPage.FilesList.eq(MyPage.CurrentFileIndex).text());
	MyPage.AjaxCallSettings.data = MyPage.ScriptForm.serialize();
	$.ajax(MyPage.AjaxCallSettings);
}

function ScriptFileProcessed()
{
	MyPage.FilesList.eq(MyPage.CurrentFileIndex).hide();
	MyPage.CurrentFileIndex++;
	if (MyPage.CurrentFileIndex < MyPage.FilesList.length)
	{
		var progressPercentage = MyPage.CurrentFileIndex / MyPage.FilesList.length * 100;
		MyPage.progressBar.css("width", progressPercentage + "%");
		ProcessNextScript();
	}
	else
	{
		MyPage.progressBar.css("width", "100%");
		MyPage.progressBar.removeClass("active");
		MyPage.progressBar.removeClass("progress-bar-striped");
		$("#Next").slideDown();
		$("#TsqlFiles h4").text("Database initialization steps were all completed");
	}
}

function Page_Start()
{
	MyPage.FilesList = $("#TsqlFiles p");
	MyPage.progressBar = $("#ProgressIndicator .progress-bar");
	MyPage.progressLabel = $("#ProgressIndicator .progress-bar .sr-only");
	MyPage.CurrentFileIndex = 0;
	MyPage.ScriptForm = $("#frmScript");
	MyPage.ScriptFileNameCtl = $("#frmScript input[name='ScriptFileName']");
	MyPage.AjaxCallSettings = {
			url: MyPage.ScriptForm.attr("action"),
			type: "post",
			cache: false,
			success: ScriptFileProcessed,
			error: function() { window.alert("Some error occured while processing the following file:\n\n" + MyPage.FilesList.eq(MyPage.CurrentFileIndex).text()); }
		};
}

$(Page_Start);