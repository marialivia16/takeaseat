var MyPageDOM = {};



function PutHrefToLinks_Users(item)
{
	var itemLink = item.find("td:nth-child(1) a");
	var recordId = item.find("td:last-child").text();
	itemLink.attr("href", MyPageDOM.ItemViewURL_User + recordId);
}
function PutHrefToLinks_Roles(item)
{
	var itemLink = item.find("a");
	var recordId = itemLink.text();
	itemLink.attr("href", MyPageDOM.ItemViewURL_Role + recordId);
}



function Page_Start()
{
	if (document.getElementById("hdnViewLink_User"))
	{
		MyPageDOM.ItemViewURL_User = document.getElementById("hdnViewLink_User").value;
		MyPageDOM.ItemViewURL_User = MyPageDOM.ItemViewURL_User.substr(0, MyPageDOM.ItemViewURL_User.indexOf("=") + 1);
		$("table.datagrid tbody tr").each(function() { PutHrefToLinks_Users($(this)); });
	}
	MyPageDOM.ItemViewURL_Role = document.getElementById("hdnViewLink_Role").value;
	MyPageDOM.ItemViewURL_Role = MyPageDOM.ItemViewURL_Role.substr(0, MyPageDOM.ItemViewURL_Role.indexOf("=") + 1);
	$("ul#ApplicationRoles li").each(function() { PutHrefToLinks_Roles($(this)); });
}



$(Page_Start);
