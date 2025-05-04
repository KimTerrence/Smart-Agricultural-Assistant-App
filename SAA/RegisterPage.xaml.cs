

using Microsoft.Data.Sqlite;

namespace SAA;

public partial class RegisterPage : ContentPage
{
    string dbPath;
    public RegisterPage()
	{
		InitializeComponent();
        dbPath = Path.Combine(FileSystem.AppDataDirectory, "SAA.db");
        CreateTable();
    }

    void CreateTable()
    {
        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            firstname TEXT,
            lastname TEXT,
            email TEXT,
            password TEXT
        )";
        cmd.ExecuteNonQuery();
    }


    public async void HandleRegister(object sender, EventArgs e)
	{
        string firstname = FirstNameEntry.Text;
        string lastname = LastNameEntry.Text;
        string email = EmailEntry.Text;
        string password = PasswordEntry.Text;
        if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please fill in all fields.", "OK");
            return;
        }
        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO users (firstname, lastname, email, password)
            VALUES ($firstname, $lastname, $email, $password)";
        cmd.Parameters.AddWithValue("$firstname", firstname);
        cmd.Parameters.AddWithValue("$lastname", lastname);
        cmd.Parameters.AddWithValue("$email", email);
        cmd.Parameters.AddWithValue("$password", password);
        try
        {
            cmd.ExecuteNonQuery();
            await DisplayAlert("Success", "Registration successful!", "OK");

            //await DisplayAlert("DB Path", dbPath, "OK");

            await Navigation.PushAsync(new LandingPage()); // Navigate to Login Page
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Registration failed: {ex.Message}", "OK");
        }
    }

    public async void GoToLogin(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LandingPage()); // Navigate to Register Page
    }
}