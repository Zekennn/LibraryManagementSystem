namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// A registered user of the library - either a Member (can borrow + refer)
    /// or a Visitor (can only refer, cannot borrow).
    /// </summary>
    public class LibraryUser
    {
        public string UserNumber { get; set; } = string.Empty;   // e.g. U0001
        public string Name { get; set; } = string.Empty;
        public Sex Sex { get; set; }
        public string NationalIdentityCardNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public UserType Type { get; set; } = UserType.Member;

        public override string ToString()
        {
            return $"{UserNumber} - {Name} ({Type})";
        }
    }
}