using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using MvcPaging;
using TakeASeatApp.Models;
using TakeASeatApp.Utils;
using TakeASeatApp.ViewModels;
using System.Threading.Tasks;
using System.IO;

namespace TakeASeatApp.Controllers
{
    public class RestaurantsController : Controller
    {
        private readonly DatabaseContext _db = new DatabaseContext();

        [Authorize(Roles = "Manager")]
        public ActionResult Index()
        {

            string userId = User.Identity.GetUserId();
            Managers manager = _db.Managers.FirstOrDefault(m => m.UserId == userId);
            AspNetUsers user = _db.AspNetUsers.FirstOrDefault(u => u.Id == userId);
            ViewBag.AppMessage = "";
            if (user != null && !user.EmailConfirmed)
            {
                AppMessage msg = new AppMessage();
                msg.AppendMessage("You have to confirm your email to perform this action. Go to the profile page and resend a confirmation email, if necessary.");
                ViewBag.AppMessage = msg.ToHtmlError();
                return View();
            }
            if (manager == null) return View();
            Restaurants restaurant = _db.Restaurants.FirstOrDefault(r => r.Id == manager.RestaurantId);

            List<Managers> managerIds = _db.Managers.Where(m => m.RestaurantId == restaurant.Id).ToList();
            List<AspNetUsers> managers = managerIds.Select(mId => _db.AspNetUsers.FirstOrDefault(u => u.Id == mId.UserId)).ToList();

            List<TagsForRestaurant> restTags = _db.TagsForRestaurant.Where(rt => rt.RestaurantId == restaurant.Id).ToList();
            List<Tags> tags = restTags.Select(rTag => _db.Tags.FirstOrDefault(t => t.Id == rTag.TagId)).ToList();
            IndexRestaurantViewModel restaurantViewModel = new IndexRestaurantViewModel
            {
                Restaurant = restaurant,
                Managers = managers,
                Tags = tags
            };
            return View(restaurantViewModel);
        }

        // GET: Restaurants/Details/5
        [Authorize(Roles = "Client")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurants result = _db.Restaurants.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }

        [Authorize(Roles = "Manager")]
        public ActionResult AddRestaurant()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public PartialViewResult AddRestaurant(Restaurants rest)
        {
            Restaurants restaurant = _db.Restaurants.FirstOrDefault(r => r.Id == rest.Id);
            if (restaurant == null)
            {
                restaurant = new Restaurants
                {
                    Id = rest.Id,
                    Name = rest.Name,
                    Latitude = rest.Latitude,
                    Longitude = rest.Longitude,
                    Verified = "NO"
                };
                _db.Restaurants.Add(restaurant);
                _db.SaveChanges();
                Managers manager = new Managers
                {
                    UserId = User.Identity.GetUserId(),
                    RestaurantId = rest.Id
                };
                _db.Managers.Add(manager);
                _db.SaveChanges();
                return PartialView("_ConfirmAddRestaurant", restaurant);
            }
            else
            {
                int count = _db.Managers.Count(m => m.RestaurantId == restaurant.Id);
                if (count >= 3)
                {
                    AppMessage msg = new AppMessage();
                    msg.AppendMessage("This restaurant already has 3 managers!");
                    ViewBag.AppMessage = msg.ToHtmlError();
                    return PartialView();
                }
                Managers manager = new Managers
                {
                    UserId = User.Identity.GetUserId(),
                    RestaurantId = rest.Id,
                };
                _db.Managers.Add(manager);
                _db.SaveChanges();
                return PartialView("_ConfirmAddRestaurant");
            }
        }

        [Authorize(Roles = "Manager")]
        public ActionResult Edit(string restId)
        {
            if (restId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurants restaurant = _db.Restaurants.FirstOrDefault(r => r.Id == restId);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            //List<TagsForRestaurant> tagIds = _db.TagsForRestaurant.Include(t => t.Tags).Where(x => x.RestaurantId == restId).ToList();
            //List<Tags> tagsRest = (from tagId in tagIds where tagId.RestaurantId == restId select _db.Tags.FirstOrDefault(t => t.Id == tagId.TagId)).ToList();
            List<Tags> tags = _db.Tags.ToList();
            List<Tags> tagsForRest = new List<Tags>();
            foreach (Tags tag in tags)
            {
                if (_db.TagsForRestaurant.FirstOrDefault(tr => tr.TagId == tag.Id && tr.RestaurantId == restId) != null)
                {
                    tagsForRest.Add(tag);
                }
            }
            EditRestaurantViewModel restaurantViewModel = new EditRestaurantViewModel
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Description = restaurant.Description,
                PhoneNumber = restaurant.PhoneNumber,
                WebAddress = restaurant.WebAddress,
                TagsList = tags,
                TagsForRestaurant = tagsForRest
            };
            return View(restaurantViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public ActionResult Edit(EditRestaurantViewModel restaurantViewModel)
        {
            List<Tags> tags = _db.Tags.ToList();
            List<Tags> tagsForRest = new List<Tags>();
            foreach (Tags tag in tags)
            {
                if (_db.TagsForRestaurant.FirstOrDefault(tr => tr.TagId == tag.Id && tr.RestaurantId == restaurantViewModel.Id) != null)
                {
                    tagsForRest.Add(tag);
                }
            }
            restaurantViewModel.TagsList = tags;
            restaurantViewModel.TagsForRestaurant = tagsForRest;
            //if (!ModelState.IsValid) return View(restaurantViewModel);
            AppMessage msg = new AppMessage();
            Restaurants oldRestaurant = _db.Restaurants.FirstOrDefault(r => r.Id == restaurantViewModel.Id);
            if (oldRestaurant != null)
            {
                oldRestaurant.Name = restaurantViewModel.Name;
                oldRestaurant.Description = restaurantViewModel.Description;
                oldRestaurant.PhoneNumber = restaurantViewModel.PhoneNumber;
                oldRestaurant.WebAddress = restaurantViewModel.WebAddress;
                TryUpdateModel(oldRestaurant);
                List<TagsForRestaurant> restTags =
                    _db.TagsForRestaurant.Where(tr => tr.RestaurantId == restaurantViewModel.Id).ToList();
                _db.TagsForRestaurant.RemoveRange(restTags);
                foreach (string tagName in restaurantViewModel.TagsToAdd)
                {
                    Tags tag = _db.Tags.FirstOrDefault(t => t.Name == tagName);
                    if (tag != null) //existing tag
                    {
                        _db.TagsForRestaurant.Add(new TagsForRestaurant()
                        {
                            TagId = tag.Id,
                            RestaurantId = restaurantViewModel.Id
                        });
                        msg.AppendMessage("tag deja in bd");
                        ViewBag.AppMessage = msg.ToHtmlSuccess();
                    }
                    else
                    {
                        _db.Tags.Add(new Tags() { Name = tagName });
                        _db.SaveChanges();
                        tag = _db.Tags.FirstOrDefault(t => t.Name == tagName);
                        _db.TagsForRestaurant.Add(new TagsForRestaurant()
                        {
                            TagId = tag.Id,
                            RestaurantId = restaurantViewModel.Id
                        });
                        msg.AppendMessage("tag nu e in bd");
                        ViewBag.AppMessage = msg.ToHtmlSuccess();
                    }
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            msg.AppendMessage("oldRestaurant e null");
            ViewBag.AppMessage = msg.ToHtmlError();
            return View(restaurantViewModel);
        }

        [HttpPost]
        public JsonResult UploadImage(string id)
        {
            try
            {
                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        // get a stream
                        var stream = fileContent.InputStream;
                        // and optionally write the file to disk
                        //string extension = Path.GetExtension(Path.GetFileName(file)); 
                        var fileName = id + ".jpg";
                        var path = Path.Combine(Server.MapPath("~/Content/Images/RestaurantImage"), fileName);
                        using (var fileStream = System.IO.File.Create(path))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }

            return Json("File uploaded successfully");
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurants restaurants = _db.Restaurants.Find(id);
            string crrUser = User.Identity.GetUserId();
            List<Managers> managerIds = _db.Managers.Where(m => m.RestaurantId == id && m.UserId != crrUser).ToList();
            List<AspNetUsers> managers = managerIds.Select(mId => _db.AspNetUsers.FirstOrDefault(u => u.Id == mId.UserId)).ToList();
            DeleteRestaurantViewModel restaurantViewModel = new DeleteRestaurantViewModel
            {
                Restaurant = restaurants,
                Managers = managers
            };
            if (restaurants == null)
            {
                return HttpNotFound();
            }
            return View(restaurantViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Restaurants restaurants = _db.Restaurants.Find(id);
            string userId = User.Identity.GetUserId();
            IQueryable<Managers> managers = _db.Managers.Where(m => m.RestaurantId == id && m.UserId != userId);
            IQueryable<Managers> managersDelete = _db.Managers.Where(m => m.RestaurantId == id);
            _db.Managers.RemoveRange(managersDelete);
            IQueryable<TagsForRestaurant> tags = _db.TagsForRestaurant.Where(t => t.RestaurantId == id);
            _db.TagsForRestaurant.RemoveRange(tags);
            _db.SaveChanges();
            _db.Restaurants.Remove(restaurants);
            _db.SaveChanges();
            string[] emails = _db.AspNetUsers.Where(m => managers.Select(x => x.UserId).Contains(m.Id)).Select(m => m.Email).ToArray();
            #region Send Email to notify
            EmailHelper emailHelper = new EmailHelper();
            Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
            AspNetUsers manager = _db.AspNetUsers.FirstOrDefault(m => m.Id == userId);
            personalizationStrings.Add("[#_Manager_Name_#]", manager.FirstName + " " + manager.LastName);
            personalizationStrings.Add("[#_ManagerEmail_#]", manager.Email);
            personalizationStrings.Add("[#_Restaurant_Name_#]", restaurants.Name);
            personalizationStrings.Add("[#_Profile_URL_#]", Url.Action("Index", "Account", null, Request.Url.Scheme));
            string[] to = emails;
            string[] cc = null;
            string[] bcc = null;
            string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
            string messageContentFilePath = "~/Content/Email_Templates/Restaurant/DeleteRestaurant.htm";
            List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath, to, cc, bcc, personalizationStrings);
            #endregion
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public ActionResult DeleteManager()
        {
            string userId = User.Identity.GetUserId();
            Managers manager = _db.Managers.FirstOrDefault(m => m.UserId == userId);
            Restaurants restaurant = _db.Restaurants.FirstOrDefault(r => r.Id.Equals(manager.RestaurantId));

            DeleteManagerRestaurantViewModel restaurantViewModel = new DeleteManagerRestaurantViewModel
            {
                Restaurant = restaurant,
                ManagerName = User.Identity.Name
            };
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            return View(restaurantViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteManager(string id)
        {
            string userId = User.Identity.GetUserId();
            Managers manager = _db.Managers.FirstOrDefault(m => m.UserId == userId);
            _db.Managers.Remove(manager);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Client")]
        public ActionResult Search()
        {
            string id = User.Identity.GetUserId();
            AspNetUsers user = _db.AspNetUsers.FirstOrDefault(u => u.Id == id);
            ViewBag.AppMessage = "";
            if (user != null && user.EmailConfirmed == false)
            {
                AppMessage msg = new AppMessage();
                msg.AppendMessage("You have to confirm your email to perform this action. Go to the profile page and resend a confirmation email, if necessary.");
                ViewBag.AppMessage = msg.ToHtmlError();
                return View();
            }
            List<Tags> tags = _db.Tags.ToList();
            SearchRestaurantViewModel viewModel = new SearchRestaurantViewModel { TagsList = tags };
            return View(viewModel);
        }

        //private List<Restaurants> _results;
        [HttpPost]
        [Authorize(Roles = "Client")]
        public PartialViewResult SearchResults(SearchRestaurantViewModel viewModel)
        {
            decimal latitude = Decimal.Parse(viewModel.Latitude, CultureInfo.InvariantCulture);
            decimal longitude = Decimal.Parse(viewModel.Longitude, CultureInfo.InvariantCulture);
            GeoCoordinate coord = new GeoCoordinate((double)latitude, (double)longitude);
            double radius = viewModel.Distance * 1000;
            List<SearchResultRestaurantViewModel> finalResults = new List<SearchResultRestaurantViewModel>();
            List<Restaurants> nearByRest = _db.Restaurants.Include(r => r.Managers).ToList();
            //List<Restaurants> results = new List<Restaurants>();
            foreach (Restaurants restaurant in nearByRest)
            {
                double latBd = (double)restaurant.Latitude;
                double lngDb = (double)restaurant.Longitude;
                double dist = new GeoCoordinate(latBd, lngDb).GetDistanceTo(coord);
                if (dist <= radius && restaurant.Managers.Count > 0)
                {

                    //results.Add(restaurant);
                    List<int> tagsForRestIds = _db.TagsForRestaurant.Where(t => t.RestaurantId == restaurant.Id).Select(t => t.TagId).ToList();
                    List<string> tagsForRest = tagsForRestIds.Select(tagId => _db.Tags.Where(t => t.Id == tagId).Select(t => t.Name).FirstOrDefault()).ToList();
                    finalResults.Add(new SearchResultRestaurantViewModel()
                    {
                        Restaurant = restaurant,
                        Tags = tagsForRest,
                        DistanceToInput = dist
                    });
                }
            }
            finalResults = finalResults.OrderBy(f => f.DistanceToInput).ToList();
            if (viewModel.TagsToAdd.Count == 0)
            {
                Session["restaurants"] = finalResults;
                return PartialView("_SearchResult", finalResults.ToPagedList(0, 3));
            }
            List<SearchResultRestaurantViewModel> partialResults = finalResults;
            finalResults = new List<SearchResultRestaurantViewModel>();
            finalResults = partialResults.Where(p => p.Tags.Any(t => viewModel.TagsToAdd.Contains(t))).ToList();
            var userId = User.Identity.GetUserId();
            foreach (var tagAdd in viewModel.TagsToAdd)
            {
                int tagId = _db.Tags.Where(t => t.Name == tagAdd).Select(t => t.Id).FirstOrDefault();
                var pairUserTag = _db.TagsForUser.FirstOrDefault(ut => ut.UserId == userId && ut.TagId == tagId);
                if (pairUserTag == null) //nu a mai cautat tag-ul inainte
                {
                    _db.TagsForUser.Add(new TagsForUser()
                    {
                        TagId = tagId,
                        UserId = userId,
                        Count = 1
                    });
                }
                else
                {
                    pairUserTag.Count += 1;
                    TryUpdateModel(pairUserTag);
                }
            }
            _db.SaveChanges();
            Session["restaurants"] = finalResults;
            return PartialView("_SearchResult", finalResults.ToPagedList(0, 3));
            #region old
            //if (viewModel.TagsToAdd.Count != 0)
            //{
            //    nearByRest = new List<Restaurants>();
            //    finalResults = new List<SearchResultRestaurantViewModel>();
            //    foreach (Restaurants restaurant in results)
            //    {
            //        List<int> tagIds = _db.TagsForRestaurant.Where(t => t.RestaurantId == restaurant.Id).Select(t => t.TagId).ToList();
            //        foreach (string tagAdd in viewModel.TagsToAdd)
            //        {
            //            int tagId = _db.Tags.Where(t => t.Name == tagAdd).Select(t => t.Id).FirstOrDefault();
            //            if (!tagIds.Contains(tagId) || nearByRest.Contains(restaurant)) continue;
            //            nearByRest.Add(restaurant);
            //            //List<int> tagsForRestIds = _db.TagsForRestaurant.Where(t => t.RestaurantId == restaurant.Id).Select(t => t.TagId).ToList();
            //            List<string> tagsForRest = tagIds.Select(tId => _db.Tags.Where(t => t.Id == tId).Select(t => t.Name).FirstOrDefault()).ToList();
            //            finalResults.Add(new SearchResultRestaurantViewModel()
            //            {
            //                Restaurant = restaurant,
            //                Tags = tagsForRest
            //            });
            //        }
            //    }
            //    foreach (var tagAdd in viewModel.TagsToAdd)
            //    {
            //        int tagId = _db.Tags.Where(t => t.Name == tagAdd).Select(t => t.Id).FirstOrDefault();
            //        var pairUserTag = _db.TagsForUser.FirstOrDefault(ut => ut.UserId == userId && ut.TagId == tagId);
            //        if (pairUserTag == null) //nu a mai cautat tag-ul inainte
            //        {
            //            _db.TagsForUser.Add(new TagsForUser()
            //            {
            //                TagId = tagId,
            //                UserId = userId,
            //                Count = 1
            //            });
            //        }
            //        else
            //        {
            //            pairUserTag.Count += 1;
            //            TryUpdateModel(pairUserTag);
            //        }
            //    }
            //    _db.SaveChanges();
            //    Session["restaurants"] = finalResults;
            //    return PartialView("_SearchResult", finalResults.ToPagedList(0, 3));
            //}
            //Session["restaurants"] = results;
            //return PartialView("_SearchResult", finalResults.ToPagedList(0, 3));
            #endregion
        }

        public ActionResult PaginationAjax(int? page)
        {
            int currentPageIndex = page.HasValue ? page.Value - 1 : 0;
            return PartialView("_SearchResult", (Session["restaurants"] as List<SearchResultRestaurantViewModel>).ToPagedList(currentPageIndex, 3));
        }

        public GeoCoordinate ParseGeoCoordinate(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                throw new ArgumentException("input");
            }

            if (input == "Unknown")
            {
                return GeoCoordinate.Unknown;
            }

            // GeoCoordinate.ToString() uses InvariantCulture, so the doubles will use '.'
            // for decimal placement, even in european environments
            string[] parts = input.Split(',');

            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid format");
            }

            double latitude = Double.Parse(parts[0], CultureInfo.InvariantCulture);
            double longitude = Double.Parse(parts[1], CultureInfo.InvariantCulture);

            return new GeoCoordinate(latitude, longitude);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        [Authorize(Roles = "Client")]
        public ActionResult Recommendations()
        {
            List<RecommendationsRestaurantViewModel> result = new List<RecommendationsRestaurantViewModel>();
            string userId = User.Identity.GetUserId();
            var userTags = _db.TagsForUser.Where(t => t.UserId == userId).ToList();
            var restaurants = _db.Restaurants.Include(r => r.TagsForRestaurant).ToList();
            var total = userTags.Sum(u => u.Count);
            foreach (var restaurant in restaurants)
            {
                int sum = 0;
                foreach (var userTag in userTags)
                {
                    int isInRestTags = 0;
                    if (restaurant.TagsForRestaurant.Select(t => t.TagId).Contains(userTag.TagId))
                        isInRestTags = 1;
                    //foreach (var restTag in restaurant.TagsForRestaurant)
                    //{
                    //    if (restTag.TagId == userTag.Id) isInRestTags = 1;
                    //}
                    sum += userTag.Count * isInRestTags;
                }
                double mean = (double)sum / total;
                if (!(mean >= 0.4)) continue;
                var tags =
                    _db.TagsForRestaurant.Include(t => t.Tags)
                        .Where(t => t.RestaurantId == restaurant.Id)
                        .Select(t => t.Tags.Name)
                        .ToList();
                result.Add(new RecommendationsRestaurantViewModel()
                {
                    Restaurant = restaurant,
                    Tags = tags,
                    Rating = mean
                });
            }
            result = result.OrderBy(r => r.Rating).ToList();
            Session["restaurants"] = result;
            return View(result.ToPagedList(0, 3));
        }
    }
}
