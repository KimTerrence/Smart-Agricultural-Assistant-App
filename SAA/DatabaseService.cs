using SQLite;
using System.IO;

namespace SAA
{
    public static class DatabaseService
    {
        private static SQLiteConnection _database;
        private static SQLiteConnection _detectionDb;

        public static SQLiteConnection GetUserDatabase()
        {
            if (_database == null)
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "users.db");
                _database = new SQLiteConnection(dbPath);
                _database.CreateTable<User>();
            }
            return _database;
        }

        public static SQLiteConnection GetDetectionDatabase()
        {
            if (_detectionDb == null)
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "detections.db");
                _detectionDb = new SQLiteConnection(dbPath);
                _detectionDb.CreateTable<DetectionLog>();
            }
            return _detectionDb;
        }
    }

    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string FieldLocations { get; set; }
        public bool IsSynced { get; set; }
    }

    public class DetectionLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PestName { get; set; }
        public double? Confidence { get; set; }
        public DateTime DetectionTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImagePath { get; set; }
        public bool IsSynced { get; set; }
    }
}