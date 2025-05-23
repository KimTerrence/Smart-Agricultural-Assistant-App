namespace SAA;

public partial class PestDetailPage : ContentPage
{
    public PestDetailPage(string pestName)
    {
        InitializeComponent();

        PestNameLabel.Text = pestName;

        // You can replace this with your own content per pest
        var descriptions = new Dictionary<string, string>
        {
            { "brown-planthopper", "A pest that sucks sap from rice plants and causes hopper burn." },
            { "green-leafhopper", "Transmits viruses like tungro; found on the underside of leaves." },
            { "leaf-folder", "Larvae fold leaves and feed inside, reducing photosynthesis." },
            { "rice-bug", "Feeds on developing grains, causing empty or spotted grains." },
            { "stem-borer", "Larvae bore into stems, causing whiteheads or dead hearts." },
            { "whorl-maggot", "Feeds on young rice plant whorls, stunting growth." },
        };

        PestDescriptionLabel.Text = descriptions.ContainsKey(pestName) ? descriptions[pestName] : "No information available.";
    }
}
