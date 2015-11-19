


function Page_Start()
{
	$("input[type='text'], textarea").each(
			function()
			{
				if ($(this).val() == "")
				{
					$(this).val($(this).attr("data-default"));
				}
			}
		);
	$("#ConfigSettings select").each(
			function()
			{
				if ($(this).attr("data-selected").length == 0)
				{
					$(this).attr("data-selected", $(this).attr("data-default"));
				}
			}
		);
	TakeASeatApp.SyncronizeDropdown();
}



$(Page_Start);
