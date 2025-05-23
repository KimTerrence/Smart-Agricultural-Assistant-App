namespace SAA;

public partial class PestDetailPage : ContentPage
{
    public PestDetailPage(string pestName, string description)
    {
        InitializeComponent();

        PestNameLabel.Text = pestName;
        PestDescriptionLabel.Text = description;
    }
}
