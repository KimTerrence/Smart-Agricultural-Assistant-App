using System.Threading.Tasks;

namespace SAA;

public partial class LandingPage : ContentPage
{
	public LandingPage()
	{
		InitializeComponent();
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (username == "admin" && password == "1234") // Example login
        {
            await DisplayAlert("Success", "Logged in successfully!", "OK");
            await Navigation.PushAsync(new MainPage()); // Navigate to your MainPage
        }
        else
        {
            await DisplayAlert("Error", "Invalid username or password.", "OK");
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Forgot Password", "Reset password feature coming soon!", "OK");
    }

    private async void GoToRegister(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage()); // Navigate to Register Page
    }
}