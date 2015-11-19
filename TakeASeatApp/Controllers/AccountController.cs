using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TakeASeatApp.Models;
using TakeASeatApp.Utils;
using TakeASeatApp.ViewModels;
using System.Data.Entity;

namespace TakeASeatApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly DatabaseContext _db = new DatabaseContext();

        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            IndexAccountViewModel viewModel = new IndexAccountViewModel();
            viewModel.UserRoles = UserManager.GetRoles(userId);
            viewModel.UserProfile = UserManager.FindById(this.User.Identity.GetUserId());
            if (viewModel.UserProfile == null)
            {
                AppMessage msg = new AppMessage();
                msg.AppendMessage("The user account does not exist.");
                ViewBag.AppMessage = msg.ToHtmlError();
                viewModel.SuggestedPassword = SecurityHelper.GeneratePassword();
                viewModel.UserRoles = UserManager.GetRoles(userId).ToList();
                return View("Message");
            }
            return View(viewModel);
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            LoginAccountViewModel viewModel = new LoginAccountViewModel();
            ViewBag.ReturnUrl = returnUrl;
            return View(viewModel);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginAccountViewModel viewModel, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            //ApplicationUser userProfile = await UserManager.FindByEmailAsync(viewModel.Username);
            //if (userProfile == null)
            //{
            //    ModelState.AddModelError("", "Invalid login attempt.");
            //    return View(viewModel);
            //}
            //if (userProfile.IsBlocked)
            //{
            //    ModelState.AddModelError("", "The account is currently blocked.");
            //    return View(viewModel);
            //}
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            SignInStatus result = await SignInManager.PasswordSignInAsync(viewModel.Username, viewModel.Password, viewModel.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = viewModel.RememberMe });
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(viewModel);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordAccountViewModel viewModel)
        {

            ApplicationUser userProfile = UserManager.FindByEmail(viewModel.EmailAddress);
            if (userProfile != null)
            {
                #region SendEmail
                EmailHelper emailHelper = new EmailHelper();
                string passwordResetToken = UserManager.GeneratePasswordResetToken(userProfile.Id);
                Dictionary<string, string> personalizationStrings = new Dictionary<string, string>
            {
                {"[#_User_Name_#]", userProfile.UserName},
                {"[#_Password_Reset_Token_#]", passwordResetToken},
                {"[#_First_Name_#]", userProfile.FirstName},
                {"[#_Last_Name_#]", userProfile.LastName},
                {"[#_Email_Address_#]", userProfile.Email},
                {
                    "[#_Password_Reset_URL_#]",
                    Url.Action("ResetPassword", "Account", new {User = userProfile.Id, ResetToken = passwordResetToken},
                        protocol: Request.Url.Scheme)
                },
                {
                    "[#_Password_Reset_Page_#]",
                    Url.Action("ResetPassword", "Account", routeValues: null, protocol: Request.Url.Scheme)
                },
                {
                    "[#_Parameterized_Message_To_Receivers_#]",
                    Resources.Account.ForgotPassword.Message_Email_Notification
                }
            };
                string[] to = { userProfile.Email };
                string[] cc = null;
                string[] bcc = null;
                string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
                string messageContentFilePath = "~/Content/Email_Templates/Account/ForgotPassword/PasswordResetToken.htm";
                List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath, to, cc, bcc, personalizationStrings);
                if (potentialErrors.Count == 0)
                {
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
                AppMessage msg = new AppMessage();
                msg.AppendMessage(Resources.Account.ForgotPassword.Error_EmailNotificationFailed);
                msg.AppendRaw("<ul>");
                foreach (string error in potentialErrors)
                {
                    msg.AppendRaw(string.Format("<li>{0}</li>", error));
                }
                msg.AppendRaw("</ul>");
                ViewBag.AppMessage = msg.ToHtmlError();
                #endregion
                return View(viewModel);
            }
            else
            {
                AppMessage msg = new AppMessage();
                msg.AppendMessage("The email doesn't exist. Try again.");
                ViewBag.AppMessage = msg.ToHtmlError();
                return View(viewModel);
            }
        }
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ViewResult ResetPassword(string resetToken)
        {
            ResetPasswordAccountViewModel viewModel = new ResetPasswordAccountViewModel();
            viewModel.PasswordResetToken = resetToken;
            return View(viewModel);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordAccountViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.NewPassword) || viewModel.NewPassword != viewModel.PasswordConfirm)
            {
                ModelState.AddModelError("", "The new password is missing or the two passwords don't match");
            }
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            ApplicationUser userProfile = await UserManager.FindByNameAsync(viewModel.Username);
            if (userProfile == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            IdentityResult result = await UserManager.ResetPasswordAsync(userProfile.Id, viewModel.PasswordResetToken, viewModel.NewPassword);
            if (result.Succeeded)
            {
                #region Send Email to Remind
                EmailHelper emailHelper = new EmailHelper();
                if (emailHelper.IsValidEmailAddress(userProfile.Email))
                {
                    Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
                    personalizationStrings.Add("[#_User_Name_#]", userProfile.UserName);
                    personalizationStrings.Add("[#_Password_#]", viewModel.NewPassword);
                    personalizationStrings.Add("[#_First_Name_#]", userProfile.FirstName);
                    personalizationStrings.Add("[#_Last_Name_#]", userProfile.LastName);
                    personalizationStrings.Add("[#_Email_Address_#]", userProfile.Email);
                    personalizationStrings.Add("[#_Parameterized_Message_To_Receivers_#]", Resources.Account.ResetPassword.Message_EmailNotification);
                    string[] to = new string[] { userProfile.Email };
                    string[] cc = null;
                    string[] bcc = null;
                    string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
                    string messageContentFilePath = "~/Content/Email_Templates/Account/ResetPassword/NewPassword.htm";
                    List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath, to, cc, bcc, personalizationStrings);
                    if (potentialErrors.Count > 0)
                    {
                        AppMessage msg = new AppMessage();
                        msg.AppendMessage(Resources.Account.ResetPassword.Warning_EmailNotificationFailed);
                        msg.AppendRaw("<ul>");
                        foreach (string error in potentialErrors)
                        {
                            msg.AppendRaw(string.Format("<li>{0}</li>", error));
                        }
                        msg.AppendRaw("</ul>");
                        ViewBag.AppMessage = msg.ToHtmlWarning();
                    }
                }
                #endregion
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddIdentityResultErrors(result);
            return View(viewModel);
        }
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ViewResult ChangePassword()
        {
            ChangePasswordAccountViewModel model = new ChangePasswordAccountViewModel();
            return View(model);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            IdentityResult result = UserManager.ChangePassword(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                #region Send Email to remind
                EmailHelper emailHelper = new EmailHelper();
                ApplicationUser userProfile = UserManager.FindById(User.Identity.GetUserId());
                if (emailHelper.IsValidEmailAddress(userProfile.Email))
                {
                    Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
                    personalizationStrings.Add("[#_User_Name_#]", userProfile.UserName);
                    personalizationStrings.Add("[#_Password_#]", model.NewPassword);
                    personalizationStrings.Add("[#_First_Name_#]", userProfile.FirstName);
                    personalizationStrings.Add("[#_Last_Name_#]", userProfile.LastName);
                    personalizationStrings.Add("[#_Email_Address_#]", userProfile.Email);
                    personalizationStrings.Add("[#_Parameterized_Message_To_Receivers_#]", Resources.Account.ResetPassword.Message_EmailNotification);
                    string[] to = { userProfile.Email };
                    string[] cc = null;
                    string[] bcc = null;
                    string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
                    string messageContentFilePath = "~/Content/Email_Templates/Account/ResetPassword/NewPassword.htm";
                    List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath, to, cc, bcc, personalizationStrings);
                    if (potentialErrors.Count > 0)
                    {
                        AppMessage msg = new AppMessage();
                        msg.AppendMessage(Resources.Account.ResetPassword.Warning_EmailNotificationFailed);
                        msg.AppendRaw("<ul>");
                        foreach (string error in potentialErrors)
                        {
                            msg.AppendRaw(string.Format("<li>{0}</li>", error));
                        }
                        msg.AppendRaw("</ul>");
                        ViewBag.AppMessage = msg.ToHtmlWarning();
                    }
                }
                #endregion
                return RedirectToAction("Index");
            }
            AppMessage mesg = new AppMessage();
            mesg.AppendMessage("An error has occured. Make sure the information you entered is correct.");
            ViewBag.AppMessage = mesg.ToHtmlError();
            return View(model);
        }

        [AllowAnonymous]
        public ViewResult RegisterOptions()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult RegisterClient()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterClient(RegisterClientAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.Firstname, LastName = model.Lastname };
                IdentityResult result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Client");
                    SignInManager.SignIn(user, false, false);
                    #region SendConfirmationEmail
                    EmailHelper emailHelper = new EmailHelper();
                    Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
                    string code = UserManager.GenerateEmailConfirmationToken(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    personalizationStrings.Add("[#_First_Name_#]", user.FirstName);
                    personalizationStrings.Add("[#_Last_Name_#]", user.LastName);
                    personalizationStrings.Add("[#_Email_Confirmation_URL_#]", callbackUrl);
                    string[] to = { user.Email };
                    string[] cc = null;
                    string[] bcc = null;
                    string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
                    string messageContentFilePath = "~/Content/Email_Templates/Account/ConfirmEmail/ConfirmEmail.htm";
                    List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath,
                        to, cc, bcc, personalizationStrings);
                    if (potentialErrors.Count > 0)
                    {
                        AppMessage msg = new AppMessage();
                        msg.AppendMessage(Resources.Account.ResetPassword.Warning_EmailNotificationFailed);
                        msg.AppendRaw("<ul>");
                        foreach (string error in potentialErrors)
                        {
                            msg.AppendRaw(string.Format("<li>{0}</li>", error));
                        }
                        msg.AppendRaw("</ul>");
                        ViewBag.AppMessage = msg.ToHtmlWarning();
                    }
                    #endregion
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public ActionResult RegisterManager()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterManager(RegisterClientAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.Firstname, LastName = model.Lastname };
                IdentityResult result = UserManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Manager");
                    SignInManager.SignIn(user, false, false);
                    #region SendConfirmationEmail
                    EmailHelper emailHelper = new EmailHelper();
                    Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
                    string code = UserManager.GenerateEmailConfirmationToken(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    personalizationStrings.Add("[#_First_Name_#]", user.FirstName);
                    personalizationStrings.Add("[#_Last_Name_#]", user.LastName);
                    personalizationStrings.Add("[#_Email_Confirmation_URL_#]", callbackUrl);
                    string[] to = { user.Email };
                    string[] cc = null;
                    string[] bcc = null;
                    string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
                    string messageContentFilePath = "~/Content/Email_Templates/Account/ConfirmEmail/ConfirmEmail.htm";
                    List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath,
                        to, cc, bcc, personalizationStrings);
                    if (potentialErrors.Count > 0)
                    {
                        AppMessage msg = new AppMessage();
                        msg.AppendMessage(Resources.Account.ResetPassword.Warning_EmailNotificationFailed);
                        msg.AppendRaw("<ul>");
                        foreach (string error in potentialErrors)
                        {
                            msg.AppendRaw(string.Format("<li>{0}</li>", error));
                        }
                        msg.AppendRaw("</ul>");
                        ViewBag.AppMessage = msg.ToHtmlWarning();
                    }
                    #endregion
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult EditProfile()
        {
            string userId = User.Identity.GetUserId();
            AspNetUsers user = _db.AspNetUsers.Find(userId);
            AppMessage msg = new AppMessage();
            EditProfileAccountViewModel model = new EditProfileAccountViewModel();
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.Email = user.Email;
            model.PhoneNumber = user.PhoneNumber;
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditProfile(EditProfileAccountViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            AppMessage msg = new AppMessage();
            string userId = User.Identity.GetUserId();
            AspNetUsers oldModel = _db.AspNetUsers.Find(userId);
            bool diff = false;
            if (!oldModel.Email.Equals(model.Email))
            {
                oldModel.EmailConfirmed = false;
                diff = true;
            }
            oldModel.FirstName = model.FirstName;
            oldModel.LastName = model.LastName;
            oldModel.Email = model.Email;
            oldModel.UserName = model.Email;
            oldModel.PhoneNumber = model.PhoneNumber;
            _db.Entry(oldModel).State = EntityState.Modified;
            _db.SaveChanges();
            if (diff == true)
            {
                #region SendConfirmationEmail
                EmailHelper emailHelper = new EmailHelper();
                Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
                string code = UserManager.GenerateEmailConfirmationToken(User.Identity.GetUserId());
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = User.Identity.GetUserId(), code = code }, protocol: Request.Url.Scheme);
                personalizationStrings.Add("[#_First_Name_#]", oldModel.FirstName);
                personalizationStrings.Add("[#_Last_Name_#]", oldModel.LastName);
                personalizationStrings.Add("[#_Email_Confirmation_URL_#]", callbackUrl);
                string[] to = { model.Email };
                string[] cc = null;
                string[] bcc = null;
                string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
                string messageContentFilePath = "~/Content/Email_Templates/Account/ConfirmEmail/ConfirmEmail.htm";
                List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath,
                to, cc, bcc, personalizationStrings);
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
                }
                #endregion
            }

            return RedirectToAction("Index", "Account");
        }

        public ViewResult SendEmail()
        {
            ApplicationUser userProfile = UserManager.FindById(User.Identity.GetUserId());
            EmailHelper emailHelper = new EmailHelper();
            Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
            string code = UserManager.GenerateEmailConfirmationToken(User.Identity.GetUserId());
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = User.Identity.GetUserId(), code = code }, protocol: Request.Url.Scheme);
            personalizationStrings.Add("[#_First_Name_#]", userProfile.FirstName);
            personalizationStrings.Add("[#_Last_Name_#]", userProfile.LastName);
            personalizationStrings.Add("[#_Email_Confirmation_URL_#]", callbackUrl);
            string[] to = { userProfile.Email };
            string[] cc = null;
            string[] bcc = null;
            string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
            string messageContentFilePath = "~/Content/Email_Templates/Account/ConfirmEmail/ConfirmEmail.htm";
            List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath,
                to, cc, bcc, personalizationStrings);
            if (potentialErrors.Count > 0)
            {
                AppMessage msg = new AppMessage();
                msg.AppendMessage(Resources.Account.ResetPassword.Warning_EmailNotificationFailed);
                msg.AppendRaw("<ul>");
                foreach (string error in potentialErrors)
                {
                    msg.AppendRaw(string.Format("<li>{0}</li>", error));
                }
                msg.AppendRaw("</ul>");
                ViewBag.AppMessage = msg.ToHtmlWarning();
            }
            return View();
        }

        #region constructors and helpers
        public AccountController()
        {

        }
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
            return RedirectToAction("Index", "Home", routeValues: new { Area = string.Empty });
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        private void AddIdentityResultErrors(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        //private void SendConfirmEmail()
        //{
        //    ApplicationUser userProfile = UserManager.FindById(User.Identity.GetUserId());
        //    EmailHelper emailHelper = new EmailHelper();
        //    if (emailHelper.IsValidEmailAddress(userProfile.Email))
        //    {
        //        Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
        //        string code = UserManager.GenerateEmailConfirmationToken(User.Identity.GetUserId());
        //        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = User.Identity.GetUserId(), code = code }, protocol: Request.Url.Scheme);
        //        personalizationStrings.Add("[#_First_Name_#]", userProfile.FirstName);
        //        personalizationStrings.Add("[#_Last_Name_#]", userProfile.LastName);
        //        personalizationStrings.Add("[#_Email_Confirmation_URL_#]", callbackUrl);
        //        string[] to = { userProfile.Email };
        //        string[] cc = null;
        //        string[] bcc = null;
        //        string masterLayoutFilePath =
        //            emailHelper.GetLocalizedEmailTemplate("~/Content/Email_Templates/Master_Layout.htm");
        //        string messageContentFilePath =
        //            emailHelper.GetLocalizedEmailTemplate(
        //                "~/Content/Email_Templates/Account/ConfirmEmail/ConfirmEmail.htm");
        //        List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath,
        //            to, cc, bcc, personalizationStrings);
        //        if (potentialErrors.Count > 0)
        //        {
        //            AppMessage msg = new AppMessage();
        //            msg.AppendMessage(Resources.Account.ResetPassword.Warning_EmailNotificationFailed);
        //            msg.AppendRaw("<ul>");
        //            foreach (string error in potentialErrors)
        //            {
        //                msg.AppendRaw(string.Format("<li>{0}</li>", error));
        //            }
        //            msg.AppendRaw("</ul>");
        //            ViewBag.AppMessage = msg.ToHtmlWarning();
        //        }
        //    }


        //}
        #endregion

        public ActionResult SendCode(string returnurl, bool rememberme)
        {
            #region SendConfirmationEmail
            ApplicationUser userProfile = UserManager.FindById(User.Identity.GetUserId());
            EmailHelper emailHelper = new EmailHelper();
            Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
            string code = UserManager.GenerateEmailConfirmationToken(User.Identity.GetUserId());
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = User.Identity.GetUserId(), code = code }, protocol: Request.Url.Scheme);
            personalizationStrings.Add("[#_First_Name_#]", userProfile.FirstName);
            personalizationStrings.Add("[#_Last_Name_#]", userProfile.LastName);
            personalizationStrings.Add("[#_Email_Confirmation_URL_#]", callbackUrl);
            string[] to = { userProfile.Email };
            string[] cc = null;
            string[] bcc = null;
            string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
            string messageContentFilePath = "~/Content/Email_Templates/Account/ConfirmEmail/ConfirmEmail.htm";
            List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath,
                to, cc, bcc, personalizationStrings);
            if (potentialErrors.Count > 0)
            {
                AppMessage msg = new AppMessage();
                msg.AppendMessage(Resources.Account.ResetPassword.Warning_EmailNotificationFailed);
                msg.AppendRaw("<ul>");
                foreach (string error in potentialErrors)
                {
                    msg.AppendRaw(string.Format("<li>{0}</li>", error));
                }
                msg.AppendRaw("</ul>");
                ViewBag.AppMessage = msg.ToHtmlWarning();
            }
            #endregion
            return View();
        }
    }
}