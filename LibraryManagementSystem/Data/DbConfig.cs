namespace LibraryManagementSystem.Data
{
    /// <summary>
    /// Holds the SQL Server connection string used throughout the application.
    /// </summary>
    public static class DbConfig
    {
        public static string ConnectionString { get; set; } =
            @"Server=localhost;Port=3306;Database=SarasaviLibraryDB;Uid=root;Pwd=;";
    }
}