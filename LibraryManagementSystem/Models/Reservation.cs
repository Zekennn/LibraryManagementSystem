using System;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// A reservation is made against a Title (BookNumber), not a specific copy,
    /// because the member simply wants "a copy" of that title.
    /// The oldest active reservation for a title is served first (FIFO).
    /// </summary>
    public class Reservation
    {
        public string ReservationId { get; set; } = string.Empty;  // e.g. R00001
        public string BookNumber { get; set; } = string.Empty;      // FK -> Book.BookNumber
        public string UserNumber { get; set; } = string.Empty;      // FK -> LibraryUser.UserNumber
        public DateTime ReservationDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        // Filled in when a reservation is fulfilled by a returned copy.
        // Null indicates no copy has been assigned yet.
        public string HeldCopyNumber { get; set; }
    }
}