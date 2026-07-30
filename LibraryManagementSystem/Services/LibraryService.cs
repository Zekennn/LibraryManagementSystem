using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Services
{
    public class LibraryRuleException : Exception
    {
        public LibraryRuleException(string message) : base(message) { }
    }

    public class LibraryService
    {
        // Rest of your code remains exactly the same...
        private const int MaxBooksPerMember = 5;
        private const int MaxCopiesPerBook = 10;
        private const int LoanPeriodDays = 14; // two weeks

        private readonly DataStore _store;

        public LibraryService(DataStore store)
        {
            _store = store;
        }

        // ================= USER REGISTRATION =================

        public LibraryUser RegisterUser(string name, Sex sex, string nic, string address, UserType type)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new LibraryRuleException("Name is required.");
            if (string.IsNullOrWhiteSpace(nic))
                throw new LibraryRuleException("National Identity Card number is required.");
            if (_store.NicExists(nic.Trim()))
                throw new LibraryRuleException("A user with this NIC number is already registered.");

            var user = new LibraryUser
            {
                UserNumber = GenerateUserNumber(),
                Name = name.Trim(),
                Sex = sex,
                NationalIdentityCardNumber = nic.Trim(),
                Address = address.Trim(),
                Type = type
            };

            _store.InsertUser(user);
            return user;
        }

        private string GenerateUserNumber()
        {
            var userNumbers = _store.GetAllUserNumbers();
            int next = userNumbers.Count == 0
                ? 1
                : userNumbers
                    .Select(u => int.TryParse(u.TrimStart('U'), out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            return $"U{next:D4}";
        }

        // ================= BOOK REGISTRATION =================

        /// <summary>
        /// Registers a new Title and its Copies in one go.
        /// copyTypes.Count is the number of copies to create (max 10).
        /// </summary>
        public Book RegisterBook(char classification, string title, string author, string publisher,
                                  List<CopyType> copyTypes)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new LibraryRuleException("Title is required.");
            if (copyTypes == null || copyTypes.Count == 0)
                throw new LibraryRuleException("At least one copy must be registered.");
            if (copyTypes.Count > MaxCopiesPerBook)
                throw new LibraryRuleException($"A maximum of {MaxCopiesPerBook} copies are allowed per book.");

            var bookNumber = GenerateBookNumber(classification);

            var book = new Book
            {
                BookNumber = bookNumber,
                Classification = classification,
                Title = title.Trim(),
                Author = author.Trim(),
                Publisher = publisher.Trim()
            };
            _store.InsertBook(book);

            for (int i = 0; i < copyTypes.Count; i++)
            {
                var copy = new BookCopy
                {
                    CopyNumber = $"{bookNumber}-{(i + 1):D2}",
                    BookNumber = bookNumber,
                    Type = copyTypes[i],
                    Status = CopyStatus.Available
                };
                _store.InsertCopy(copy);
            }

            return book;
        }

        /// <summary>
        /// Adds extra copies to an existing title, respecting the 10-copy cap.
        /// </summary>
        public void AddCopies(string bookNumber, List<CopyType> newCopyTypes)
        {
            var book = _store.GetBookByNumber(bookNumber)
                       ?? throw new LibraryRuleException("Book not found.");

            var existingCount = _store.GetCopyCountForBook(bookNumber);
            if (existingCount + newCopyTypes.Count > MaxCopiesPerBook)
                throw new LibraryRuleException(
                    $"This book already has {existingCount} copies; cannot exceed {MaxCopiesPerBook} in total.");

            for (int i = 0; i < newCopyTypes.Count; i++)
            {
                var seq = existingCount + i + 1;
                var copy = new BookCopy
                {
                    CopyNumber = $"{bookNumber}-{seq:D2}",
                    BookNumber = bookNumber,
                    Type = newCopyTypes[i],
                    Status = CopyStatus.Available
                };
                _store.InsertCopy(copy);
            }
        }

        private string GenerateBookNumber(char classification)
        {
            var prefix = char.ToUpper(classification);
            var bookNumbers = _store.GetBookNumbersByClassification(prefix);
            int next = bookNumbers
                .Select(b => int.TryParse(b.Substring(1), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;
            return $"{prefix}{next:D4}";
        }

        // ================= LOAN PROCESS =================

        public Loan LoanBook(string userNumber, string copyNumber)
        {
            var user = _store.GetUserByNumber(userNumber)
                       ?? throw new LibraryRuleException("User not found.");

            if (user.Type != UserType.Member)
                throw new LibraryRuleException("Registered Visitors cannot borrow books - they may only refer to them.");

            var copy = _store.GetCopyByNumber(copyNumber)
                       ?? throw new LibraryRuleException("Copy not found.");

            if (copy.Type == CopyType.Reference)
                throw new LibraryRuleException("This copy is reference-only and cannot be borrowed.");

            if (copy.Status != CopyStatus.Available)
                throw new LibraryRuleException($"This copy is not available (current status: {copy.Status}).");

            // Overdue check
            if (_store.HasOverdueLoan(userNumber))
                throw new LibraryRuleException("This member has overdue book(s) and cannot borrow until they are returned.");

            // Max 5 books at a time
            var currentlyBorrowed = _store.GetActiveLoanCountForUser(userNumber);
            if (currentlyBorrowed >= MaxBooksPerMember)
                throw new LibraryRuleException($"This member has already borrowed the maximum of {MaxBooksPerMember} books.");

            var loan = new Loan
            {
                LoanId = GenerateLoanId(),
                CopyNumber = copyNumber,
                UserNumber = userNumber,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(LoanPeriodDays),
                Status = LoanStatus.Active
            };

            _store.InsertLoan(loan);
            _store.UpdateCopyStatus(copyNumber, CopyStatus.OnLoan);
            return loan;
        }

        private string GenerateLoanId()
        {
            var loanIds = _store.GetAllLoanIds();
            int next = loanIds.Count == 0
                ? 1
                : loanIds
                    .Select(l => int.TryParse(l.TrimStart('L'), out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            return $"L{next:D5}";
        }

        // ================= RETURN PROCESS =================

        /// <summary>
        /// Returns a copy. If there is an outstanding reservation for the
        /// title, the copy is held aside (status = Reserved) for the member
        /// with the oldest reservation, a notification message is returned
        /// to the caller (to show/print/email), and the reservation entry
        /// is removed (marked Fulfilled). Otherwise the copy becomes Available.
        /// </summary>
        public string ReturnBook(string copyNumber)
        {
            var copy = _store.GetCopyByNumber(copyNumber)
                       ?? throw new LibraryRuleException("Copy not found.");

            var loan = _store.GetActiveLoanForCopy(copyNumber)
                       ?? throw new LibraryRuleException("There is no active loan for this copy.");

            _store.MarkLoanReturned(loan.LoanId, DateTime.Now);

            string message = "Book returned successfully.";

            var oldestReservation = _store.GetOldestActiveReservationForBook(copy.BookNumber);

            if (oldestReservation != null)
            {
                _store.UpdateCopyStatus(copyNumber, CopyStatus.Reserved);
                _store.FulfillReservation(oldestReservation.ReservationId, copyNumber);

                var reservingUser = _store.GetUserByNumber(oldestReservation.UserNumber);
                message = $"Book returned. It is being held at the counter for reserved member " +
                          $"{reservingUser?.Name ?? oldestReservation.UserNumber} " +
                          $"(User No: {oldestReservation.UserNumber}). A notification has been generated for them.";
            }
            else
            {
                _store.UpdateCopyStatus(copyNumber, CopyStatus.Available);
            }

            return message;
        }

        // ================= RESERVATION PROCESS =================

        public Reservation ReserveBook(string userNumber, string bookNumber)
        {
            var user = _store.GetUserByNumber(userNumber)
                       ?? throw new LibraryRuleException("User not found.");

            if (user.Type != UserType.Member)
                throw new LibraryRuleException("Only Registered Members can reserve books.");

            var book = _store.GetBookByNumber(bookNumber)
                       ?? throw new LibraryRuleException("Book not found.");

            if (_store.HasActiveReservation(userNumber, bookNumber))
                throw new LibraryRuleException("This member already has an active reservation for this title.");

            var reservation = new Reservation
            {
                ReservationId = GenerateReservationId(),
                BookNumber = bookNumber,
                UserNumber = userNumber,
                ReservationDate = DateTime.Now,
                Status = ReservationStatus.Active
            };

            _store.InsertReservation(reservation);
            return reservation;
        }

        public void CancelReservation(string reservationId)
        {
            _store.CancelReservation(reservationId);
        }

        private string GenerateReservationId()
        {
            var reservationIds = _store.GetAllReservationIds();
            int next = reservationIds.Count == 0
                ? 1
                : reservationIds
                    .Select(r => int.TryParse(r.TrimStart('R'), out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            return $"R{next:D5}";
        }

        // ================= INQUIRY PROCESS =================

        public class InquiryResult
        {
            public Book Book { get; set; } = null!;
            public int TotalCopies { get; set; }
            public int AvailableCopies { get; set; }
            public int OnLoanCopies { get; set; }
            public int ReservedCopies { get; set; }
            public int ReferenceOnlyCopies { get; set; }
        }

        /// <summary>
        /// Search by book number, copy (accession) number, or part/whole of title or author.
        /// </summary>
        public List<InquiryResult> Inquire(string query)
        {
            query = (query ?? string.Empty).Trim();

            List<Book> matches;

            if (string.IsNullOrEmpty(query))
            {
                matches = _store.GetAllBooks();
            }
            else
            {
                var found = new List<Book>();

                // exact book number
                var byBookNumber = _store.GetBookByNumber(query);
                if (byBookNumber != null)
                    found.Add(byBookNumber);

                // via copy/accession number
                var copyMatch = _store.GetCopyByNumber(query);
                if (copyMatch != null)
                {
                    var bookForCopy = _store.GetBookByNumber(copyMatch.BookNumber);
                    if (bookForCopy != null)
                        found.Add(bookForCopy);
                }

                // partial title / author match
                found.AddRange(_store.SearchBooksByTitleOrAuthor(query));

                matches = found.GroupBy(b => b.BookNumber).Select(g => g.First()).ToList();
            }

            return matches.Select(b =>
            {
                var copies = _store.GetCopiesForBook(b.BookNumber);
                return new InquiryResult
                {
                    Book = b,
                    TotalCopies = copies.Count,
                    AvailableCopies = copies.Count(c => c.Status == CopyStatus.Available && c.Type == CopyType.Borrowable),
                    OnLoanCopies = copies.Count(c => c.Status == CopyStatus.OnLoan),
                    ReservedCopies = copies.Count(c => c.Status == CopyStatus.Reserved),
                    ReferenceOnlyCopies = copies.Count(c => c.Type == CopyType.Reference)
                };
            }).ToList();
        }

        // ================= HELPERS FOR FORMS =================

        public List<LibraryUser> GetAllUsers() => _store.GetAllUsers();
        public List<Book> GetAllBooks() => _store.GetAllBooks();
        public List<BookCopy> GetCopiesForBook(string bookNumber) => _store.GetCopiesForBook(bookNumber);
        public List<BookCopy> GetAvailableBorrowableCopies() => _store.GetAvailableBorrowableCopies();
        public List<Loan> GetActiveLoansForUser(string userNumber) => _store.GetActiveLoansForUser(userNumber);
        public List<Loan> GetAllActiveLoans() => _store.GetAllActiveLoans();
        public List<Reservation> GetActiveReservations() => _store.GetActiveReservations();
    }
}
