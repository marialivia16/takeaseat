using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TakeASeatApp.Models;

namespace TakeASeatApp.ViewModels
{
    public class IndexReservationViewModel
    {
        public List<Reservations> PendingList { get; set; }
        public List<Reservations> AcceptedList { get; set; }
        public List<Reservations> RejectedList { get; set; }
        public List<Reservations> HistoryList { get; set; }
    }

    public class ChangeStatusReservationViewModel
    {
        public Reservations Reservation { get; set; }
        public string NewStatus { get; set; }
    }

    public class ChangeStatusPostReservationViewModel
    {
        public int ReservationId { get; set; }
        public string NewStatus { get; set; }
    }
}