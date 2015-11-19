var MyPageDOM = {};



function CurrentRoles_Loaded()
{
	$("#CurrentRoles li a").click(function() { RoleRemove_Click($(this)); });
}
function RoleRemove_Click(linkButton)
{
	var roleName = linkButton.attr("data-record");
	if (!confirm("The following role will be removed from the user profile. Are you sure?\n\n" + roleName)) return;
	MyPageDOM.frmRoleRemove.find("input[name='RoleNameToRemove']").val(roleName);
	MyPageDOM.frmRoleRemove.submit();
}
function RoleAdd_Click(linkButton)
{
	var roleName = linkButton.text();
	MyPageDOM.frmRoleAdd.find("input[name='RoleNameToAdd']").val(roleName);
	MyPageDOM.frmRoleAdd.submit();
}



function Page_Start()
{
	CurrentRoles_Loaded();
	$("#ModalAvailableRoles li a").click(function() { RoleAdd_Click($(this)); });
	MyPageDOM.frmRoleRemove = $("#frmRoleRemove");
	MyPageDOM.frmRoleAdd = $("#frmRoleAdd");
	MyPageDOM.ModalResetPassword = $("#ModalResetPassword form");
	TakeASeatApp.AjaxifyForm(MyPageDOM.frmRoleAdd).Target("#CurrentRolesList").Progress("#CurrentRoles progress").OnSuccess(CurrentRoles_Loaded);
	TakeASeatApp.AjaxifyForm(MyPageDOM.frmRoleRemove).Target("#CurrentRolesList").Progress("#CurrentRoles progress").OnSuccess(CurrentRoles_Loaded);
	$("input[name='Password']").siblings("span").children("a").click(
			function()
			{
				var suggestedPassword = $(this).text();
				$("input[name='NewPassword']").val(suggestedPassword);
				$("input[name='PasswordConfirm']").val(suggestedPassword);
			}
		);
	TakeASeatApp.AjaxifyForm(MyPageDOM.ModalResetPassword).Target("#ResetPasswordResultMessage").Progress("#AccountFields progress");
}



$(Page_Start);
