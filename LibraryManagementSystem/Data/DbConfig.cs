namespace LibraryManagementSystem.Data
{
    /// <summary>
    /// Holds the SQL Server connection string used throughout the application.
    /// </summary>
    public static class DbConfig
    {
        public static string ConnectionString { get; set; } =
            @"Server=.\SQLEXPRESS;Database=LibraryManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";
    }
}