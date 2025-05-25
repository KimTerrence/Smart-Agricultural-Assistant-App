using System;
using System.Linq;
using System.IO;
using Microsoft.Maui.Controls;
using SQLite;
using System.Timers; // Add this explicitly to avoid ambiguity
using Timer = System.Timers.Timer;

namespace SAA
{
    public partial class LandingPage : ContentPage
    {
        private SQLiteConnection _database;
        private int _carouselPosition = 0;
        private Timer _carouselTimer;

        public LandingPage()
        {
            InitializeComponent();
            StartCarouselAutoSlide();
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
                _database.CreateTable<RegisterPage.User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Init Error: {ex.Message}");
            }
        }

        private void CheckIfUserExists()
        {
            if (_database == null) return;

            try
            {
                bool userExists = _database.Table<RegisterPage.User>().Any();
                if (userExists)
                {
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Query Error: {ex.Message}");
            }
        }

        private void Start(object sender, EventArgs e)
        {
            _carouselTimer?.Stop();
            Application.Current.MainPage = new NavigationPage(new RegisterPage());
        }

        private void StartCarouselAutoSlide()
        {
            _carouselTimer = new Timer(3000); // 3 seconds
            _carouselTimer.Elapsed += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _carouselPosition = (_carouselPosition + 1) % 3;
                    ImageCarousel.Position = _carouselPosition;
                });
            };
            _carouselTimer.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _carouselTimer?.Stop();
        }
    }
}
