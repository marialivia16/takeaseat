var MyPageDOM = {};



function PutHrefToLinksUsers(tr)
{
	
	var itemLink = tr.find("td:nth-child(1) a");
	var recordId = tr.find("td:last-child").text();
	itemLink.attr("href", MyPageDOM.ItemViewURL + recordId);
}



function Page_Start()
{
	if (document.getElementById("hdnViewLinkUser"))
	{
		MyPageDOM.ItemViewURL = document.getElementById("hdnViewLinkUser").value;
		MyPageDOM.ItemViewURL = MyPageDOM.ItemViewURL.substr(0, MyPageDOM.ItemViewURL.indexOf("=") + 1);
		$("table.table tbody tr").each(function() { PutHrefToLinksUsers($(this)); });
	}
}



$(Page_Start);
