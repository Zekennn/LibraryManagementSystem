using System;

namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// A loan of a specific copy to a specific member.
    /// A copy may be borrowed for a period of two weeks.
    /// </summary>
    public class Loan
    {
        public string LoanId { get; set; } = string.Empty;       // e.g. L00001
        public string CopyNumber { get; set; } = string.Empty;   // FK -> BookCopy.CopyNumber
        public string UserNumber { get; set; } = string.Empty;   // FK -> LibraryUser.UserNumber
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Active;

        public bool IsOverdue
        {
            get
            {
                return Status == LoanStatus.Active &&
                       DateTime.Now.Date > DueDate.Date;
            }
        }
    }
}