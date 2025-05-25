using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;

namespace SAA
{
    public partial class RegisterPage : ContentPage
    {
        private SQLiteConnection _database;

        public RegisterPage()
        {
            InitializeComponent();
            InitDatabase();
            CheckIfUserExists();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                Task.Run(async () => await TrySyncUnsyncedUsers());
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
            public bool IsSynced { get; set; }  // Flag for sync status
        }

        private void InitDatabase()
        {
            try
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "users.db");
                _database = new SQLiteConnection(dbPath);
                _database.CreateTable<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Init Error: {ex.Message}");
            }
        }

        private void CheckIfUserExists()
        {
            if (_database == null)
                return;

            try
            {
                bool userExists = _database.Table<User>().Any();
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

        private void AddLocation(object sender, EventArgs e)
        {
            var newEntry = new Entry
            {
                Placeholder = "Field Location",
                PlaceholderColor = Color.FromArgb("#888"),
                BackgroundColor = Color.FromArgb("#dce1de"),
                TextColor = Color.FromArgb("#0d0a0b"),
                HeightRequest = 40
            };

            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#dce1de"),
                CornerRadius = 25,
                Padding = 5,
                HasShadow = false,
                BorderColor = Colors.Transparent,
                Content = newEntry
            };

            FieldLocationsContainer.Children.Add(frame);
        }

        private async void HandleRegister(object sender, EventArgs e)
        {
            if (_database == null)
            {
                await DisplayAlert("Error", "Database is not initialized.", "OK");
                return;
            }

            try
            {
                if (_database.Table<User>().Any())
                {
                    await DisplayAlert("Already Registered", "You are already registered. Redirecting...", "OK");
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Query Error: {ex.Message}");
            }

            string firstName = FirstNameEntry.Text?.Trim();
            string lastName = LastNameEntry.Text?.Trim();
            string email = EmailEntry.Text?.Trim();
            string address = AddressEntry.Text?.Trim();

            var fieldLocations = new List<string>();
            foreach (var view in FieldLocationsContainer.Children)
            {
                if (view is Frame frame && frame.Content is Entry entry && !string.IsNullOrWhiteSpace(entry.Text))
                {
                    fieldLocations.Add(entry.Text.Trim());
                }
            }

            string fieldLocationCsv = string.Join(",", fieldLocations);

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Address = address,
                FieldLocations = fieldLocationCsv,
                IsSynced = false // Mark as unsynced on insert
            };

            try
            {
                _database.Insert(user);
                await DisplayAlert("Success", "User registered successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to save data: " + ex.Message, "OK");
                return;
            }

            // Clear fields
            FirstNameEntry.Text = LastNameEntry.Text = EmailEntry.Text = AddressEntry.Text = string.Empty;
            FieldLocationsContainer.Children.Clear();

            // Attempt to sync right after registration
            await TrySyncUnsyncedUsers();

            Application.Current.MainPage = new NavigationPage(new MainPage());
        }

        public async Task TrySyncUnsyncedUsers()
        {
            try
            {
                var current = Connectivity.NetworkAccess;
                if (current != NetworkAccess.Internet)
                    return;

                var unsyncedUsers = _database.Table<User>().Where(u => !u.IsSynced).ToList();

                foreach (var user in unsyncedUsers)
                {
                    try
                    {
                        using var conn = new MySqlConnection("server=192.168.100.29;uid=root;pwd=;database=saa_db;");
                        await conn.OpenAsync();

                        var cmd = new MySqlCommand("INSERT INTO users (firstname, lastname, email, address, fieldlocations) VALUES (@firstName, @lastName, @email, @address, @fieldLocations)", conn);
                        cmd.Parameters.AddWithValue("@firstName", user.FirstName);
                        cmd.Parameters.AddWithValue("@lastName", user.LastName);
                        cmd.Parameters.AddWithValue("@email", user.Email);
                        cmd.Parameters.AddWithValue("@address", user.Address);
                        cmd.Parameters.AddWithValue("@fieldLocations", user.FieldLocations);

                        await cmd.ExecuteNonQueryAsync();

                        user.IsSynced = true;
                        _database.Update(user);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert(",",$"Sync failed for user {user.Email}: {ex.Message}","ok");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(",", $"error {ex.Message}", "ok");
            }
        }
    }
}
