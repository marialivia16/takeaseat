using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MvcPaging;
using TakeASeatApp.Utils;
using TakeASeatApp.Models;
using TakeASeatApp.Resources.Users;
using TakeASeatApp.ViewModels;

namespace TakeASeatApp.Controllers
{
	[Authorize]
	public class UsersController : Controller
	{
	    readonly DatabaseContext _db = new DatabaseContext();
        [Authorize(Roles = "Administrator")]
		public ActionResult Index()
        {
            IQueryable<IdentityRole> roles = RolesManager.Roles;
            foreach (IdentityRole role in roles)
            {
                
            }

            IdentityRole roleInfo = RolesManager.FindByName("Client");
            List<ApplicationUser> clientList = UserManager.Users.ToList().Where(user => UserManager.IsInRole(user.Id, roleInfo.Name)).OrderBy(m => m.LastName).ToList();

            roleInfo = RolesManager.FindByName("Manager");
            List<ApplicationUser> managerList = UserManager.Users.ToList().Where(user => UserManager.IsInRole(user.Id, roleInfo.Name)).OrderBy(m => m.LastName).ToList();

            IndexUsersViewModel viewModel = new IndexUsersViewModel
            {
                ClientUsers = clientList.ToPagedList(0, 3),
                ManagerUsers = managerList.ToPagedList(0, 3)
            };
            return View(viewModel);
		}

        public ActionResult ClientsPaginationAjax(int? page)
        {
            int currentPageIndex = page.HasValue ? page.Value - 1 : 0;
            IdentityRole roleInfo = RolesManager.FindByName("Client");
            List<ApplicationUser> clientList = UserManager.Users.ToList().Where(user => UserManager.IsInRole(user.Id, roleInfo.Name)).OrderBy(m => m.LastName).ToList();
            return PartialView("_clientsResult", clientList.ToPagedList(currentPageIndex, 3));
        }


        public ActionResult ManagersPaginationAjax(int? page)
        {
            int currentPageIndex = page.HasValue ? page.Value - 1 : 0;
            IdentityRole roleInfo = RolesManager.FindByName("Manager");
            List<ApplicationUser> managerList = UserManager.Users.ToList().Where(user => UserManager.IsInRole(user.Id, roleInfo.Name)).OrderBy(m => m.LastName).ToList();
            return PartialView("_managersResult", managerList.ToPagedList(currentPageIndex, 3));
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUsers user = _db.AspNetUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            Restaurants restaurant = _db.Restaurants.FirstOrDefault(r => _db.Managers.Any(m => m.UserId == r.Id));
            DeleteUsersViewModel viewModel = new DeleteUsersViewModel
            {
                User = user,
                Restaurant = restaurant
            };

            return View(viewModel);
        }
        
        [HttpPost]
		[Authorize(Roles = "Administrator")]
		[ValidateAntiForgeryToken]
		public async Task<PartialViewResult> Account_LockState(string userId, bool lockState = true)
		{
			AppMessage msg = new AppMessage();
			ApplicationUser userProfile = await UserManager.FindByIdAsync(userId);
			userProfile.IsBlocked = lockState;
			IdentityResult identityResult = await UserManager.UpdateAsync(userProfile);
			if (identityResult.Succeeded)
			{
				msg.AppendMessage(string.Format("The user account has been successfully {0}.", lockState ? "blocked away" : "unblocked and re-enabled"));
				ViewBag.AppMessage = msg.ToHtmlSuccess();
				return PartialView("_Message");
			}
            msg.AppendMessage(string.Format("The user account could not be {0}.", lockState ? "blocked away" : "unblocked and re-enabled"));
            foreach (string error in identityResult.Errors)
            {
                msg.AppendMessage(error);
            }
            ViewBag.AppMessage = msg.ToHtmlError();
            return PartialView("_Message");
		}
        
        

        [HttpPost, ActionName("Delete")]
		[Authorize(Roles = "Administrator")]
        public ActionResult DeleteConfirm(string id, string reasonText)
		{
            AppMessage msg = new AppMessage();
            ApplicationUser userProfile = UserManager.FindById(id);
            IdentityResult identityResult = UserManager.Delete(userProfile);
            if (identityResult.Succeeded)
            {
                Managers manager = _db.Managers.FirstOrDefault(m => m.UserId == userProfile.Id);
                if (manager != null)
                {
                    _db.Managers.Remove(manager);
                    _db.SaveChanges();
                }
                msg.AppendMessage("The user account has been successfully deleted, completely removed.");
                ViewBag.AppMessage = msg.ToHtmlSuccess();
                #region Send Email notification
                EmailHelper emailHelper = new EmailHelper();
                if (emailHelper.IsValidEmailAddress(userProfile.Email))
                {
                    Dictionary<string, string> personalizationStrings = new Dictionary<string, string>
                    {
                        {"[#_First_Name_#]", userProfile.FirstName},
                        {"[#_Last_Name_#]", userProfile.LastName},
                        {"[#_Delete_Reason_#]", reasonText}
                    };
                    string[] to = { userProfile.Email };
                    string[] cc = null;
                    string[] bcc = null;
                    const string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
                    const string messageContentFilePath = "~/Content/Email_Templates/Users/Delete/Notification.htm";
                    List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath, to, cc, bcc, personalizationStrings);
                    if (potentialErrors.Count > 0)
                    {
                        msg.AppendMessage(Resources.Account.ResetPassword.Warning_EmailNotificationFailed);
                        msg.AppendRaw("<ul>");
                        foreach (string error in potentialErrors)
                        {
                            msg.AppendRaw(string.Format("<li>{0}</li>", error));
                        }
                        msg.AppendRaw("</ul>");
                        ViewBag.AppMessage = msg.ToHtmlWarning();
                        return View();
                    }
                }
                #endregion
                return RedirectToAction("Index");
            }
            msg.AppendMessage("The user account could not be deleted or removed from database.");
            foreach (string error in identityResult.Errors)
            {
                msg.AppendMessage(error);
            }
            ViewBag.AppMessage = msg.ToHtmlError();
            return View();
		}

		#region constructors and helpers
		public UsersController()
		{

		}
		public UsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
		{
			this.UserManager = userManager;
			this.SignInManager = signInManager;
		}
		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				this._userManager = value;
			}
		}
		public ApplicationSignInManager SignInManager
		{
			get
			{
				return this._signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set { _signInManager = value; }
		}
		public RoleManager<IdentityRole> RolesManager
		{
			get
			{
				return this._rolesManager ?? new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());
			}
			private set { _rolesManager = value; }
		}
		private ApplicationUserManager _userManager;
		private ApplicationSignInManager _signInManager;
		RoleManager<IdentityRole> _rolesManager;
		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index", "Home");
		}
		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return HttpContext.GetOwinContext().Authentication;
			}
		}
		#endregion
	}
}