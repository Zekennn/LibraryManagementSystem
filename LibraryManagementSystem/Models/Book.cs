namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// A Title: the class of all books which are identical (e.g. same edition/ISBN).
    /// BookNumber format: X9999 -> 1-byte Classification + 4-digit sequence number
    /// starting at 0001 for each classification, e.g. "F0001".
    /// </summary>
    public class Book
    {
        public string BookNumber { get; set; } = string.Empty;   // e.g. F0001
        public char Classification { get; set; }                 // e.g. 'F' = Fiction
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{BookNumber} - {Title} ({Author})";
        }
    }
}