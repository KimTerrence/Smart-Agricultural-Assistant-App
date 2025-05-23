using SQLite;
using System;
using System.IO;
using Microsoft.Maui.Controls;
using System.Linq;

namespace SAA
{
    public partial class LandingPage : ContentPage
    {
        private SQLiteConnection _database;

        public LandingPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            InitDatabase();
            CheckIfUserExists();
        }

        private void InitDatabase()
        {
            try
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "users.db");
                _database = new SQLiteConnection(dbPath);
                _database.CreateTable<RegisterPage.User>(); // Reuse User model from RegisterPage
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Init Error: {ex.Message}");
                // You can optionally show an alert here
            }
        }

        private void CheckIfUserExists()
        {
            if (_database == null)
                return;

            try
            {
                bool userExists = _database.Table<RegisterPage.User>().Any();
                if (userExists)
                {
                    // Navigate to MainPage if user exists
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                // Else, stay here and let user tap Get Started
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Query Error: {ex.Message}");
                // Table might not exist or DB issue — stay on LandingPage safely
            }
        }

        private void Start(object sender, EventArgs e)
        {
            // Navigate to RegisterPage when user clicks Get Started
            Application.Current.MainPage = new NavigationPage(new RegisterPage());
        }
    }
}
