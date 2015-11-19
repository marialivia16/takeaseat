using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using TakeASeatApp.Data;
using TakeASeatApp.Utils;
using TakeASeatApp.Models;
using TakeASeatApp.ViewModels;

namespace TakeASeatApp.Controllers
{
	public class SetupController : Controller
	{
		public ActionResult Index()
		{
            if (!App.IsInstalled)
            {
                return View();
            }
            else
            {
				if (!RolesManager.RoleExists("Administrator")) return RedirectToAction("AdminAccount");
				if (RolesManager.FindByName("Administrator").Users.Count == 0) return RedirectToAction("AdminAccount");
				else return RedirectToAction("Confirm_Settings");
			}
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult Install()
		{
			InstallSetupViewModel viewModel = new InstallSetupViewModel();
			DirectoryInfo directoryInfoAppData = new DirectoryInfo(Server.MapPath("~/App_Data/"));
			viewModel.TsqlScriptFolders = new List<DirectoryInfo>();
			viewModel.TsqlScriptFiles = new List<FileInfo>();
			foreach (DirectoryInfo directoryInfo in directoryInfoAppData.GetDirectories("Setup_*"))
			{
				viewModel.TsqlScriptFolders.Add(directoryInfo);
				foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.sql"))
				{
					viewModel.TsqlScriptFiles.Add(fileInfo);
				}
			}
			return View(viewModel);
		}
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public string Install_RunScript(string scriptFileName)
		{
			string transactSqlScript;
			using (StreamReader streamReader = new StreamReader(Server.MapPath("~/App_Data/" + scriptFileName)))
			{
				transactSqlScript = streamReader.ReadToEnd();
			}
			using (Repository db = new Repository())
			{
				Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				string[] sqlStatements = regex.Split(transactSqlScript);
				foreach (string sqlStatement in sqlStatements)
				{
					string sqlStatementTrimmed = sqlStatement.Trim();
					if (!string.IsNullOrEmpty(sqlStatementTrimmed)) db.ExecuteTextCommand(sqlStatementTrimmed);
				}
			}
			return "OK";
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult AdminAccount()
		{
			ApplicationUser applicationAddministrator = UserManager.FindByName("Administrator");
			if (applicationAddministrator == null)
			{
			    AdminAccountSetupViewModel viewModel = new AdminAccountSetupViewModel
			    {
			        SuggestedPassword = SecurityHelper.GeneratePassword()
			    };
			    return View(viewModel);
			}
		    return RedirectToAction("Confirm_Settings");
		}
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult AdminAccount(AdminAccountSetupViewModel viewModel)
		{
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
			AppMessage msg = new AppMessage();
			if (!RolesManager.RoleExists("Administrator")) { RolesManager.Create(new IdentityRole("Administrator")); }
            if (!RolesManager.RoleExists("Client")) { RolesManager.Create(new IdentityRole("Client")); }
            if (!RolesManager.RoleExists("Manager")) { RolesManager.Create(new IdentityRole("Manager")); }
			ApplicationUser applicationAddministrator = UserManager.FindByName("Administrator");
			if (applicationAddministrator == null)
			{
			    applicationAddministrator = new ApplicationUser
			    {
			        UserName = viewModel.EmailAddress,
			        FirstName = "Master",
			        LastName = "User",
			        Email = viewModel.EmailAddress,
			        EmailConfirmed = true
			    };
			    IdentityResult identityResult = UserManager.Create(applicationAddministrator, viewModel.Password);
				if (identityResult.Succeeded)
				{
					UserManager.AddToRole(applicationAddministrator.Id, "Administrator");
					SignInManager.SignIn(applicationAddministrator, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("Confirm_Settings");
				}
			    msg.AppendMessage("The application failed to create the built-in ADMINISTRATOR account. Please consult error messages below and try to fix the input.");
			    foreach (string error in identityResult.Errors)
			    {
			        msg.AppendMessage(error);
			    }
			    ViewBag.AppMessage = msg.ToHtmlError();
			    viewModel.SuggestedPassword = SecurityHelper.GeneratePassword();
			    return View(viewModel);
			}
            return RedirectToAction("Confirm_Settings");
		}

		[HttpGet]
		[Authorize(Roles = "Administrator")]
		public ViewResult Confirm_Settings()
		{
			return View();
		}

		#region constructors and helpers
		public SetupController()
		{

		}
		public SetupController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
		{
			UserManager = userManager;
			SignInManager = signInManager;
		}
		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}
		public ApplicationSignInManager SignInManager
		{
			get
			{
				return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set { _signInManager = value; }
		}
		public RoleManager<IdentityRole> RolesManager
		{
			get
			{
				return _rolesManager ?? new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());
			}
			private set { _rolesManager = value; }
		}
		private ApplicationUserManager _userManager;
		private ApplicationSignInManager _signInManager;
		RoleManager<IdentityRole> _rolesManager;
		#endregion
	}
}