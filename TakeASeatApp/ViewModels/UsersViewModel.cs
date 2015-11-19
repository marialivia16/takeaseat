using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using MvcPaging;
using TakeASeatApp.Models;

namespace TakeASeatApp.ViewModels
{

	public class IndexUsersViewModel
	{
        public IPagedList<ApplicationUser> ClientUsers { get; set; }

        public IPagedList<ApplicationUser> ManagerUsers { get; set; }
	}

    public class DeleteUsersViewModel
    {
        public AspNetUsers User { get; set; }

        public Restaurants Restaurant { get; set; }

    }
}