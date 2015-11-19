using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TakeASeatApp.Models;

namespace TakeASeatApp.ViewModels
{
    public class IndexRestaurantViewModel
    {
        public Restaurants Restaurant { get; set; }

        public List<AspNetUsers> Managers { get; set; }

        public List<Tags> Tags { get; set; }
    }
    public class EditRestaurantViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Web Address")]
        //[Url(ErrorMessage="Not a valid URL")]
        //[RegularExpression(@"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$", ErrorMessage = "Not a valid Web Address. Don't forget to use http://")]
        public string WebAddress { get; set; }

        public List<Tags> TagsList { get; set; }

        public List<Tags> TagsForRestaurant { get; set; }

        public string[] TagsToAdd { get; set; }
    }
    public class DeleteRestaurantViewModel
    {
        public Restaurants Restaurant { get; set; }
        public List<AspNetUsers> Managers { get; set; }
    }

    public class DeleteManagerRestaurantViewModel
    {
        public Restaurants Restaurant { get; set; }
        public string ManagerName { get; set; }
    }
    public class SearchRestaurantViewModel
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public double Distance { get; set; }
        public List<Tags> TagsList { get; set; }
        public List<string> TagsToAdd { get; set; }
        public SearchRestaurantViewModel()
        {
            TagsToAdd = new List<string>();
        }
    }
    public class SearchResultRestaurantViewModel
    {
        public Restaurants Restaurant { get; set; }
        public double DistanceToInput { get; set; }
        public List<string> Tags { get; set; }
    }
    public class RecommendationsRestaurantViewModel
    {
        public Restaurants Restaurant { get; set; }
        public List<string> Tags { get; set; }
        public double Rating { get; set; }
    }
}