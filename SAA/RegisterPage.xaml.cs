namespace SAA;

public partial class RegisterPage : ContentPage
{
	public RegisterPage()
	{
		InitializeComponent();
	}

	public async void HandleRegister(object sender, EventArgs e)
	{
		await DisplayAlert("", "Register", "Ok");
        await Navigation.PushAsync(new LandingPage()); // Navigate to Register Page
    }

    public async void GoToLogin(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LandingPage()); // Navigate to Register Page
    }
}