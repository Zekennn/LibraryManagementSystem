using LibraryManagementSystem.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace LibraryManagementSystem.Data
{
    /// <summary>
    /// MySQL data access layer (ADO.NET), using MySqlConnector.
    /// Every method here talks directly to the SarasaviLibraryDB database
    /// (see Database/CreateDatabase_MySQL.sql), running on MySQL via WAMP.
    ///
    /// LibraryService (the business-logic layer) only calls these public
    /// methods - it never writes raw SQL itself - so this class is the single
    /// place responsible for all database access.
    /// </summary>
    public class DataStore
    {
        // Shared instance so every Form/Service uses the same DataStore object.
        // (The object itself is stateless - it just knows the connection string -
        // so a singleton is only a convenience, not a cache.)
        private static DataStore? _instance;
        public static DataStore Instance => _instance ??= new DataStore();

        private MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();
            return conn;
        }

        // ================================================================
        // USERS
        // ================================================================

        public List<LibraryUser> GetAllUsers()
        {
            const string sql = "SELECT UserNumber, Name, Sex, NationalIdentityCardNumber, Address, UserType " +
                                "FROM Users ORDER BY UserNumber";
            var result = new List<LibraryUser>();
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(ReadUser(reader));
            return result;
        }

        public LibraryUser? GetUserByNumber(string userNumber)
        {
            const string sql = "SELECT UserNumber, Name, Sex, NationalIdentityCardNumber, Address, UserType " +
                                "FROM Users WHERE UserNumber = @UserNumber";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserNumber", userNumber);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadUser(reader) : null;
        }

        public bool NicExists(string nic)
        {
            const string sql = "SELECT COUNT(*) FROM Users WHERE NationalIdentityCardNumber = @Nic";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Nic", nic);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>All existing UserNumbers, used by LibraryService to work out the next one.</summary>
        public List<string> GetAllUserNumbers()
        {
            const string sql = "SELECT UserNumber FROM Users";
            return ReadStringColumn(sql);
        }

        public void InsertUser(LibraryUser user)
        {
            const string sql = @"INSERT INTO Users (UserNumber, Name, Sex, NationalIdentityCardNumber, Address, UserType)
                                  VALUES (@UserNumber, @Name, @Sex, @Nic, @Address, @UserType)";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserNumber", user.UserNumber);
            cmd.Parameters.AddWithValue("@Name", user.Name);
            cmd.Parameters.AddWithValue("@Sex", user.Sex.ToString());
            cmd.Parameters.AddWithValue("@Nic", user.NationalIdentityCardNumber);
            cmd.Parameters.AddWithValue("@Address", user.Address);
            cmd.Parameters.AddWithValue("@UserType", user.Type.ToString());
            cmd.ExecuteNonQuery();
        }

        private static LibraryUser ReadUser(MySqlDataReader reader) => new()
        {
            UserNumber = reader.GetString(0),
            Name = reader.GetString(1),
            Sex = (Sex)Enum.Parse(typeof(Sex), reader.GetString(2)),
            NationalIdentityCardNumber = reader.GetString(3),
            Address = reader.GetString(4),
            Type = (UserType)Enum.Parse(typeof(UserType), reader.GetString(5))
        };

        // ================================================================
        // BOOKS
        // ================================================================

        public List<Book> GetAllBooks()
        {
            const string sql = "SELECT BookNumber, Classification, Title, Author, Publisher FROM Books ORDER BY BookNumber";
            var result = new List<Book>();
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(ReadBook(reader));
            return result;
        }

        public Book? GetBookByNumber(string bookNumber)
        {
            const string sql = "SELECT BookNumber, Classification, Title, Author, Publisher FROM Books WHERE BookNumber = @BookNumber";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BookNumber", bookNumber);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadBook(reader) : null;
        }

        /// <summary>Existing BookNumbers for one classification letter, used to generate the next sequence.</summary>
        public List<string> GetBookNumbersByClassification(char classification)
        {
            const string sql = "SELECT BookNumber FROM Books WHERE UPPER(Classification) = @Classification";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Classification", char.ToUpper(classification).ToString());
            var result = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(reader.GetString(0));
            return result;
        }

        /// <summary>Partial, case-insensitive match on title or author (used by the Inquiry process).</summary>
        public List<Book> SearchBooksByTitleOrAuthor(string query)
        {
            const string sql = @"SELECT BookNumber, Classification, Title, Author, Publisher FROM Books
                                  WHERE Title LIKE @Query OR Author LIKE @Query
                                  ORDER BY BookNumber";
            var result = new List<Book>();
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Query", $"%{query}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(ReadBook(reader));
            return result;
        }

        public void InsertBook(Book book)
        {
            const string sql = @"INSERT INTO Books (BookNumber, Classification, Title, Author, Publisher)
                                  VALUES (@BookNumber, @Classification, @Title, @Author, @Publisher)";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BookNumber", book.BookNumber);
            cmd.Parameters.AddWithValue("@Classification", book.Classification.ToString());
            cmd.Parameters.AddWithValue("@Title", book.Title);
            cmd.Parameters.AddWithValue("@Author", book.Author);
            cmd.Parameters.AddWithValue("@Publisher", book.Publisher);
            cmd.ExecuteNonQuery();
        }

        private static Book ReadBook(MySqlDataReader reader) => new()
        {
            BookNumber = reader.GetString(0),
            Classification = reader.GetString(1)[0],
            Title = reader.GetString(2),
            Author = reader.GetString(3),
            Publisher = reader.GetString(4)
        };

        // ================================================================
        // BOOK COPIES
        // ================================================================

        public List<BookCopy> GetCopiesForBook(string bookNumber)
        {
            const string sql = "SELECT CopyNumber, BookNumber, CopyType, Status FROM BookCopies " +
                                "WHERE BookNumber = @BookNumber ORDER BY CopyNumber";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BookNumber", bookNumber);
            var result = new List<BookCopy>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(ReadCopy(reader));
            return result;
        }

        public BookCopy? GetCopyByNumber(string copyNumber)
        {
            const string sql = "SELECT CopyNumber, BookNumber, CopyType, Status FROM BookCopies WHERE CopyNumber = @CopyNumber";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CopyNumber", copyNumber);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadCopy(reader) : null;
        }

        public List<BookCopy> GetAvailableBorrowableCopies()
        {
            const string sql = @"SELECT CopyNumber, BookNumber, CopyType, Status FROM BookCopies
                                  WHERE CopyType = 'Borrowable' AND Status = 'Available'
                                  ORDER BY CopyNumber";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            var result = new List<BookCopy>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(ReadCopy(reader));
            return result;
        }

        public int GetCopyCountForBook(string bookNumber)
        {
            const string sql = "SELECT COUNT(*) FROM BookCopies WHERE BookNumber = @BookNumber";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BookNumber", bookNumber);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void InsertCopy(BookCopy copy)
        {
            const string sql = @"INSERT INTO BookCopies (CopyNumber, BookNumber, CopyType, Status)
                                  VALUES (@CopyNumber, @BookNumber, @CopyType, @Status)";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CopyNumber", copy.CopyNumber);
            cmd.Parameters.AddWithValue("@BookNumber", copy.BookNumber);
            cmd.Parameters.AddWithValue("@CopyType", copy.Type.ToString());
            cmd.Parameters.AddWithValue("@Status", copy.Status.ToString());
            cmd.ExecuteNonQuery();
        }

        public void UpdateCopyStatus(string copyNumber, CopyStatus status)
        {
            const string sql = "UPDATE BookCopies SET Status = @Status WHERE CopyNumber = @CopyNumber";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", status.ToString());
            cmd.Parameters.AddWithValue("@CopyNumber", copyNumber);
            cmd.ExecuteNonQuery();
        }

        private static BookCopy ReadCopy(MySqlDataReader reader) => new()
        {
            CopyNumber = reader.GetString(0),
            BookNumber = reader.GetString(1),
            Type = (CopyType)Enum.Parse(typeof(CopyType), reader.GetString(2)),
            Status = (CopyStatus)Enum.Parse(typeof(CopyStatus), reader.GetString(3))
        };

        // ================================================================
        // LOANS
        // ================================================================

        public List<Loan> GetActiveLoansForUser(string userNumber)
        {
            const string sql = @"SELECT LoanId, CopyNumber, UserNumber, LoanDate, DueDate, ReturnDate, Status
                                  FROM Loans WHERE UserNumber = @UserNumber AND Status = 'Active'
                                  ORDER BY DueDate";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserNumber", userNumber);
            var result = new List<Loan>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(ReadLoan(reader));
            return result;
        }

        public List<Loan> GetAllActiveLoans()
        {
            const string sql = @"SELECT LoanId, CopyNumber, UserNumber, LoanDate, DueDate, ReturnDate, Status
                                  FROM Loans WHERE Status = 'Active' ORDER BY DueDate";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            var result = new List<Loan>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(ReadLoan(reader));
            return result;
        }

        public bool HasOverdueLoan(string userNumber)
        {
            const string sql = @"SELECT COUNT(*) FROM Loans
                                  WHERE UserNumber = @UserNumber AND Status = 'Active' AND DueDate < @Today";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserNumber", userNumber);
            cmd.Parameters.AddWithValue("@Today", DateTime.Now.Date);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public int GetActiveLoanCountForUser(string userNumber)
        {
            const string sql = "SELECT COUNT(*) FROM Loans WHERE UserNumber = @UserNumber AND Status = 'Active'";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserNumber", userNumber);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public Loan? GetActiveLoanForCopy(string copyNumber)
        {
            const string sql = @"SELECT LoanId, CopyNumber, UserNumber, LoanDate, DueDate, ReturnDate, Status
                                  FROM Loans WHERE CopyNumber = @CopyNumber AND Status <> 'Returned'";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CopyNumber", copyNumber);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadLoan(reader) : null;
        }

        public List<string> GetAllLoanIds()
        {
            const string sql = "SELECT LoanId FROM Loans";
            return ReadStringColumn(sql);
        }

        public void InsertLoan(Loan loan)
        {
            const string sql = @"INSERT INTO Loans (LoanId, CopyNumber, UserNumber, LoanDate, DueDate, ReturnDate, Status)
                                  VALUES (@LoanId, @CopyNumber, @UserNumber, @LoanDate, @DueDate, @ReturnDate, @Status)";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LoanId", loan.LoanId);
            cmd.Parameters.AddWithValue("@CopyNumber", loan.CopyNumber);
            cmd.Parameters.AddWithValue("@UserNumber", loan.UserNumber);
            cmd.Parameters.AddWithValue("@LoanDate", loan.LoanDate);
            cmd.Parameters.AddWithValue("@DueDate", loan.DueDate);
            cmd.Parameters.AddWithValue("@ReturnDate", (object?)loan.ReturnDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", loan.Status.ToString());
            cmd.ExecuteNonQuery();
        }

        public void MarkLoanReturned(string loanId, DateTime returnDate)
        {
            const string sql = @"UPDATE Loans SET ReturnDate = @ReturnDate, Status = 'Returned' WHERE LoanId = @LoanId";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ReturnDate", returnDate);
            cmd.Parameters.AddWithValue("@LoanId", loanId);
            cmd.ExecuteNonQuery();
        }

        private static Loan ReadLoan(MySqlDataReader reader) => new()
        {
            LoanId = reader.GetString(0),
            CopyNumber = reader.GetString(1),
            UserNumber = reader.GetString(2),
            LoanDate = reader.GetDateTime(3),
            DueDate = reader.GetDateTime(4),
            ReturnDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            Status = (LoanStatus)Enum.Parse(typeof(LoanStatus), reader.GetString(6))
        };

        // ================================================================
        // RESERVATIONS
        // ================================================================

        public List<Reservation> GetActiveReservations()
        {
            const string sql = @"SELECT ReservationId, BookNumber, UserNumber, ReservationDate, Status, HeldCopyNumber
                                  FROM Reservations WHERE Status = 'Active' ORDER BY ReservationDate";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            var result = new List<Reservation>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(ReadReservation(reader));
            return result;
        }

        public Reservation? GetOldestActiveReservationForBook(string bookNumber)
        {
            const string sql = @"SELECT ReservationId, BookNumber, UserNumber, ReservationDate, Status, HeldCopyNumber
                                  FROM Reservations
                                  WHERE BookNumber = @BookNumber AND Status = 'Active'
                                  ORDER BY ReservationDate LIMIT 1";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BookNumber", bookNumber);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? ReadReservation(reader) : null;
        }

        public bool HasActiveReservation(string userNumber, string bookNumber)
        {
            const string sql = @"SELECT COUNT(*) FROM Reservations
                                  WHERE UserNumber = @UserNumber AND BookNumber = @BookNumber AND Status = 'Active'";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserNumber", userNumber);
            cmd.Parameters.AddWithValue("@BookNumber", bookNumber);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<string> GetAllReservationIds()
        {
            const string sql = "SELECT ReservationId FROM Reservations";
            return ReadStringColumn(sql);
        }

        public void InsertReservation(Reservation reservation)
        {
            const string sql = @"INSERT INTO Reservations (ReservationId, BookNumber, UserNumber, ReservationDate, Status, HeldCopyNumber)
                                  VALUES (@ReservationId, @BookNumber, @UserNumber, @ReservationDate, @Status, @HeldCopyNumber)";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ReservationId", reservation.ReservationId);
            cmd.Parameters.AddWithValue("@BookNumber", reservation.BookNumber);
            cmd.Parameters.AddWithValue("@UserNumber", reservation.UserNumber);
            cmd.Parameters.AddWithValue("@ReservationDate", reservation.ReservationDate);
            cmd.Parameters.AddWithValue("@Status", reservation.Status.ToString());
            cmd.Parameters.AddWithValue("@HeldCopyNumber", (object?)reservation.HeldCopyNumber ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void FulfillReservation(string reservationId, string heldCopyNumber)
        {
            const string sql = @"UPDATE Reservations SET Status = 'Fulfilled', HeldCopyNumber = @HeldCopyNumber
                                  WHERE ReservationId = @ReservationId";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@HeldCopyNumber", heldCopyNumber);
            cmd.Parameters.AddWithValue("@ReservationId", reservationId);
            cmd.ExecuteNonQuery();
        }

        public void CancelReservation(string reservationId)
        {
            const string sql = "UPDATE Reservations SET Status = 'Cancelled' WHERE ReservationId = @ReservationId";
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ReservationId", reservationId);
            cmd.ExecuteNonQuery();
        }

        private static Reservation ReadReservation(MySqlDataReader reader) => new()
        {
            ReservationId = reader.GetString(0),
            BookNumber = reader.GetString(1),
            UserNumber = reader.GetString(2),
            ReservationDate = reader.GetDateTime(3),
            Status = (ReservationStatus)Enum.Parse(typeof(ReservationStatus), reader.GetString(4)),
            HeldCopyNumber = reader.IsDBNull(5) ? null : reader.GetString(5)
        };

        // ================================================================
        // SHARED HELPER
        // ================================================================

        private List<string> ReadStringColumn(string sql)
        {
            var result = new List<string>();
            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(reader.GetString(0));
            return result;
        }
    }
}