namespace LibraryManagementSystem.Models
{
    /// <summary>
    /// A "Registered Member" can borrow books.
    /// A "Registered Visitor" can only refer to books (cannot borrow).
    /// </summary>
    public enum UserType
    {
        Member,
        Visitor
    }

    public enum Sex
    {
        Male,
        Female
    }

    /// <summary>
    /// Whether a physical copy is for reference only (cannot be borrowed)
    /// or borrowable (can be loaned out).
    /// </summary>
    public enum CopyType
    {
        Reference,
        Borrowable
    }

    /// <summary>
    /// The current physical/lending state of a copy.
    /// </summary>
    public enum CopyStatus
    {
        Available,
        OnLoan,
        Reserved
    }

    public enum LoanStatus
    {
        Active,
        Returned,
        Overdue
    }

    public enum ReservationStatus
    {
        Active,
        Fulfilled,
        Cancelled
    }
}