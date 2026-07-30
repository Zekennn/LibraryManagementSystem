namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// A Copy: the physical book on the shelf.
    /// CopyNumber format: BookNumber + "-" + 2-digit sequence, e.g. "F0001-01".
    /// A maximum of 10 copies are allowed per BookNumber.
    /// </summary>
    public class BookCopy
    {
        public string CopyNumber { get; set; } = string.Empty;   // e.g. F0001-01
        public string BookNumber { get; set; } = string.Empty;   // FK -> Book.BookNumber
        public CopyType Type { get; set; }                       // Reference / Borrowable
        public CopyStatus Status { get; set; } = CopyStatus.Available;

        public override string ToString()
        {
            return $"{CopyNumber} [{Type}] - {Status}";
        }
    }
}