using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace TakeASeatApp.ViewModels
{

    public class AdminAccountSetupViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string PasswordConfirm { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }

        public string SuggestedPassword { get; set; }
    }

    public class InstallSetupViewModel
    {

        public List<DirectoryInfo> TsqlScriptFolders { get; set; }

        public List<FileInfo> TsqlScriptFiles { get; set; }
    }
}