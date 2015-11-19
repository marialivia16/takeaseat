using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MvcPaging;
using TakeASeatApp;
using TakeASeatApp.Models;
using TakeASeatApp.ViewModels;
using TakeASeatApp.Utils;

namespace TakeASeatApp.Controllers
{
    public class ReservationController : Controller
    {
        private readonly DatabaseContext _db = new DatabaseContext();

        [Authorize(Roles = "Client, Manager")]
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            AspNetUsers client = _db.AspNetUsers.Find(userId);
            client.NotificationCount = 0;
            TryUpdateModel(client);
            _db.SaveChanges();
            List<Reservations> reservations = new List<Reservations>();
            if (User.IsInRole("Client"))
            {
                reservations = _db.Reservations.Include(r => r.Restaurants).Where(r => r.UserId == userId && !r.Status.Equals("Canceled")).OrderBy(r => r.DateAndTime).ToList();
                List<Reservations> pendingRes = reservations.Where(r => r.Status.Equals("Pending") && r.DateAndTime.CompareTo(DateTime.Now) > 0).ToList();
                List<Reservations> acceptedRes = reservations.Where(r => r.Status.Equals("Accepted") && r.DateAndTime.CompareTo(DateTime.Now) > 0).ToList();
                List<Reservations> rejectedRes = reservations.Where(r => r.Status.Equals("Rejected") && r.DateAndTime.CompareTo(DateTime.Now) > 0).ToList();
                List<Reservations> historyRes = reservations.Where(r => r.Status.Equals("Accepted") && r.DateAndTime.CompareTo(DateTime.Now) < 0).ToList();
                IndexReservationViewModel viewModel = new IndexReservationViewModel
                {
                    AcceptedList = acceptedRes,
                    PendingList = pendingRes,
                    RejectedList = rejectedRes,
                    HistoryList = historyRes
                };
                return View(viewModel);
            }
            else
            {
                var firstOrDefault = _db.Managers.FirstOrDefault(m => m.UserId == userId);
                if (firstOrDefault != null)
                {
                    string restId = firstOrDefault.RestaurantId;
                    if (restId == null)
                    {
                        AppMessage msg = new AppMessage();
                        msg.AppendMessage("You are not a manager yet.");
                        ViewBag.AppMessage = msg.ToHtmlError();
                        return View();
                    }
                    reservations = _db.Reservations.Include(a => a.AspNetUsers).Include(re => re.Restaurants).Where(r => r.RestaurantId == restId).OrderBy(r => r.DateAndTime).ToList();
                    List<Reservations> pendingRes = reservations.Where(r => r.Status.Equals("Pending") && r.DateAndTime.CompareTo(DateTime.Now) > 0).ToList();
                    List<Reservations> acceptedRes = reservations.Where(r => r.Status.Equals("Accepted") && r.DateAndTime.CompareTo(DateTime.Now) > 0).ToList();
                    List<Reservations> rejectedRes = reservations.Where(r => r.Status.Equals("Rejected") && r.DateAndTime.CompareTo(DateTime.Now) > 0).ToList();
                    List<Reservations> historyRes = reservations.Where(r => r.Status.Equals("Accepted") && r.DateAndTime.CompareTo(DateTime.Now) < 0).ToList();
                    IndexReservationViewModel viewModel = new IndexReservationViewModel
                    {
                        AcceptedList = acceptedRes,
                        PendingList = pendingRes,
                        RejectedList = rejectedRes,
                        HistoryList = historyRes
                    };
                    return View(viewModel);
                }
                else
                {
                    AppMessage msg = new AppMessage();
                    msg.AppendMessage("You are not a manager yet.");
                    ViewBag.AppMessage = msg.ToHtmlError();
                    return View();
                }
            }

        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservations reservations = _db.Reservations.Find(id);
            if (reservations == null)
            {
                return HttpNotFound();
            }
            return View(reservations);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public void Delete(int id)
        {
            Reservations reservation = _db.Reservations.Find(id);
            string oldStatus = reservation.Status;
            reservation.Status = "Canceled";
            TryUpdateModel(reservation);
            _db.SaveChanges();
            List<string> managers = _db.Managers.Where(m => m.RestaurantId == reservation.RestaurantId).Select(m => m.UserId).ToList();
            string[] emails = _db.AspNetUsers.Where(u => managers.Contains(u.Id)).Select(u => u.Email).ToArray();
            //send notification email to managers
            #region Send Email to notify
            EmailHelper emailHelper = new EmailHelper();
            Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
            personalizationStrings.Add("[#_Restaurant_Name_#]", _db.Restaurants.FirstOrDefault(r => r.Id == reservation.RestaurantId).Name);
            string name = _db.AspNetUsers.FirstOrDefault(u => u.Id == reservation.UserId).FirstName + " " + _db.AspNetUsers.FirstOrDefault(u => u.Id == reservation.UserId).LastName;
            personalizationStrings.Add("[#_Client_Name_#]", name);
            string email = _db.AspNetUsers.FirstOrDefault(u => u.Id == reservation.UserId).Email;
            personalizationStrings.Add("[#_Client_Email_#]", email);
            personalizationStrings.Add("[#_Date_Time_#]", reservation.DateAndTime.ToString());
            personalizationStrings.Add("[#_Persons_Number_#]", reservation.NumberOfGuests.ToString());
            personalizationStrings.Add("[#_Duration_#]", (reservation.Duration / 60.0) + " hours");
            personalizationStrings.Add("[#_List_URL_#]", Url.Action("Index", "Reservation", null, Request.Url.Scheme));
            string[] to = emails;
            string[] cc = null;
            string[] bcc = null;
            string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
            string messageContentFilePath = "~/Content/Email_Templates/Reservation/Managers/DeleteReservation.htm";
            List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath, to, cc, bcc, personalizationStrings);
            #endregion
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public PartialViewResult AddReservation([Bind(Include = "RestaurantId,Duration,NumberOfGuests,DateAndTime")] Reservations reservations)
        {
            reservations.UserId = User.Identity.GetUserId();
            reservations.SentOn = DateTime.Now;
            reservations.Status = "Pending";
            _db.Reservations.Add(reservations);
            _db.SaveChanges();
            //send email to restaurant managers
            List<string> managers = _db.Managers.Where(m => m.RestaurantId == reservations.RestaurantId).Select(m => m.UserId).ToList();
            foreach (string managerId in managers)
            {
                AspNetUsers manager = _db.AspNetUsers.Find(managerId);
                manager.NotificationCount++;
                _db.Entry(manager).State = EntityState.Modified;
            }
            _db.SaveChanges();
            string[] emails = _db.AspNetUsers.Where(u => managers.Contains(u.Id)).Select(u => u.Email).ToArray();
            #region Send Email to notify
            EmailHelper emailHelper = new EmailHelper();
            Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
            personalizationStrings.Add("[#_Restaurant_Name_#]", _db.Restaurants.FirstOrDefault(r => r.Id == reservations.RestaurantId).Name);
            string name = _db.AspNetUsers.FirstOrDefault(u => u.Id == reservations.UserId).FirstName + " " + _db.AspNetUsers.FirstOrDefault(u => u.Id == reservations.UserId).LastName;
            personalizationStrings.Add("[#_Client_Name_#]", name);
            string email = _db.AspNetUsers.FirstOrDefault(u => u.Id == reservations.UserId).Email;
            personalizationStrings.Add("[#_Client_Email_#]", email);
            personalizationStrings.Add("[#_Date_Time_#]", reservations.DateAndTime.ToString());
            personalizationStrings.Add("[#_Persons_Number_#]", reservations.NumberOfGuests.ToString());
            personalizationStrings.Add("[#_Duration_#]", (reservations.Duration / 60.0) + " hours");
            personalizationStrings.Add("[#_Accept_URL_#]", Url.Action("ChangeStatus", "Reservation", new { id = reservations.Id, status = "Accepted" }, Request.Url.Scheme));
            personalizationStrings.Add("[#_Reject_URL_#]", Url.Action("ChangeStatus", "Reservation", new { id = reservations.Id, status = "Rejected" }, Request.Url.Scheme));
            personalizationStrings.Add("[#_List_URL_#]", Url.Action("Index", "Reservation", null, Request.Url.Scheme));
            string[] to = emails;
            string[] cc = null;
            string[] bcc = null;
            string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
            string messageContentFilePath = "~/Content/Email_Templates/Reservation/Managers/NewReservation.htm";
            List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath, to, cc, bcc, personalizationStrings);
            #endregion
            return PartialView("_AddReservation");
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public ActionResult ChangeStatus(int id, string status)
        {
            string managerId = User.Identity.GetUserId();
            Reservations reservation = _db.Reservations.Include(r => r.AspNetUsers).FirstOrDefault(r => r.Id == id);
            string reservManagerId = _db.Managers.FirstOrDefault(m => m.RestaurantId == reservation.RestaurantId).UserId;
            if (managerId != reservManagerId)
            {
                AppMessage msg = new AppMessage();
                msg.AppendMessage("You are not the manager of this restaurant.");
                ViewBag.AppMessage = msg.ToHtmlError();
                return View();
            }
            
            ChangeStatusReservationViewModel model = new ChangeStatusReservationViewModel();
            model.Reservation = reservation;
            model.NewStatus = status;
            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Manager")]
        public void ChangeStatus(ChangeStatusPostReservationViewModel model)
        {
            Reservations reservation = _db.Reservations.Find(model.ReservationId);
            reservation.Status = model.NewStatus;
            TryUpdateModel(reservation);
            _db.SaveChanges();
            //send notification email to client
            #region Send Email to notify
            string managerId = User.Identity.GetUserId();
            AspNetUsers client = _db.AspNetUsers.FirstOrDefault(c => c.Id == reservation.UserId);
            if (model.NewStatus.Equals("Accepted"))
            {
                client.NotificationCount++;
                _db.Entry(client).State = EntityState.Modified;
                _db.SaveChanges();
            }
            EmailHelper emailHelper = new EmailHelper();
            Dictionary<string, string> personalizationStrings = new Dictionary<string, string>();
            personalizationStrings.Add("[#_Restaurant_Name_#]", _db.Restaurants.FirstOrDefault(r => r.Id == reservation.RestaurantId).Name);
            string name = client.FirstName + " " + client.LastName;
            personalizationStrings.Add("[#_Client_Name_#]", name);
            personalizationStrings.Add("[#_Date_Time_#]", reservation.DateAndTime.ToString());
            personalizationStrings.Add("[#_Persons_Number_#]", reservation.NumberOfGuests.ToString());
            personalizationStrings.Add("[#_Duration_#]", TimeSpan.FromMinutes(reservation.Duration).ToString(@"hh\:mm") + " hours");
            personalizationStrings.Add("[#_Status_#]", reservation.Status);
            personalizationStrings.Add("[#_List_URL_#]", Url.Action("Index", "Reservation", null, Request.Url.Scheme));
            string[] to = { client.Email };
            string[] cc = null;
            string[] bcc = null;
            string masterLayoutFilePath = "~/Content/Email_Templates/Master_Layout.htm";
            string messageContentFilePath = "~/Content/Email_Templates/Reservation/Clients/ReservationStatus.htm";
            List<string> potentialErrors = emailHelper.SendEmailMessage(masterLayoutFilePath, messageContentFilePath, to, cc, bcc, personalizationStrings);
            #endregion
        }

        [HttpGet]
        [Authorize(Roles = "Client, Manager")]
        public int Notifications()
        {
            string userId = User.Identity.GetUserId();
            AspNetUsers user = _db.AspNetUsers.Find(userId);
            return user.NotificationCount;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
