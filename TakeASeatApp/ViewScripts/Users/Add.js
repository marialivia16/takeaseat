


function Page_Start()
{
	$("input[name='Password']").siblings("span").children("a").click(
			function()
			{
				var suggestedPassword = $(this).text();
				$("input[name='Password']").val(suggestedPassword);
				$("input[name='PasswordConfirm']").val(suggestedPassword);
			}
		);
}



$(Page_Start);
