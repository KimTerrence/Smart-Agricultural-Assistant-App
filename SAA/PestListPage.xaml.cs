namespace SAA;

public partial class PestListPage : ContentPage
{
    public PestListPage()
    {
        InitializeComponent();
        BuildPestCards();
    }

    private void BuildPestCards()
    {
        // Sample pest data — replace/add all pests here
        var pests = new List<(string Name, string Scientific, string Image, string Id)>
        {
            ("Brown Planthopper", "Nilaparvata lugens", "brown_planthopper.png", "brown-planthopper"),
            ("Green Leafhopper", "Nephotettix virescens", "green_leafhopper.png", "green-leafhopper"),
            ("Leaf Folder", "Cnaphalocrocis medinalis", "leaf_folder.png", "leaf-folder"),
            ("Rice Bug", "Leptocorisa oratorius", "rice_bug.png", "rice-bug"),
            ("Stem Borer", "Scirpophaga incertulas", "stem_borer.png", "stem-borer"),
            ("Whorl Maggot", "Hydrellia philippina", "whorl_maggot.png", "whorl-maggot"),
            // Add more pests as needed
        };

        int row = 0;
        int col = 0;

        // Dynamically define enough row definitions
        int totalRows = (int)Math.Ceiling(pests.Count / 2.0);
        for (int i = 0; i < totalRows; i++)
        {
            PestGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (int i = 0; i < 2; i++)
        {
            PestGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        foreach (var pest in pests)
        {
            var frame = new Frame
            {
                CornerRadius = 10,
                Padding = 10,
                BackgroundColor = Color.FromArgb("#FFFFFF"),
                Shadow = new Shadow { Brush = Brush.Black, Offset = new Point(1, 1), Opacity = 0.2f },
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Image
                        {
                            Source = pest.Image,
                            HeightRequest = 100,
                            Aspect = Aspect.AspectFill
                        },
                        new Label
                        {
                            Text = pest.Name,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 16,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        new Label
                        {
                            Text = pest.Scientific,
                            FontSize = 12,
                            TextColor = Colors.Gray,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        new Button
                        {
                            Text = "View",
                            BackgroundColor = Color.FromArgb("#4CAF50"),
                            TextColor = Colors.White,
                            Command = new Command(() => OnViewPest(pest.Id)),
                            CornerRadius = 6,
                            FontSize = 12
                        }
                    }
                }
            };

            Grid.SetRow(frame, row);
            Grid.SetColumn(frame, col);
            PestGrid.Children.Add(frame);

            col++;
            if (col > 1)
            {
                col = 0;
                row++;
            }
        }
    }

    private async void OnViewPest(string pestId)
    {
        await Navigation.PushAsync(new PestDetailPage(pestId));
    }
}
