namespace SAA;

public partial class PestDetailPage : ContentPage
{
    public PestDetailPage(string pestName)
    {
        InitializeComponent();
        PestNameLabel.Text = pestName;
        SetPestDetails(pestName);
    }

    private void SetPestDetails(string pest)
    {
        switch (pest.ToLower())
        {
            case "rice-bug":
                PestNameLabel.Text = "Rice Bug";
                PestImage.Source = "rice_bug.png";
                ScientificNameLabel.Text = "Leptocorisa oratorius";
                CommonNamesLabel.Text = "Atangya (Tagalog), Dangaw (Ilocano)";
                DescriptionLabel.Text = "Adults are greenish-brown; young nymphs are green. They camouflage on rice plants and live 30–50 days. Mating starts 7–14 days after adulthood. A female lays 200–300 eggs.";
                WhereToFindLabel.Text = "Common in all rice environments, especially rainfed and upland fields. Most active from flowering to milky grain stage.";
                DamageLabel.Text = "Nymphs and adults suck sap from grains, causing shriveling and bad odor in rice.";
                ManagementLabel.Text =
                    "Cultural:\n• Remove grassy weeds.\n• Avoid staggered planting.\n• Smoke fields, use sticky traps, handpick bugs.\n• Use resistant rice varieties.\n\n" +
                    "Biological:\n• Small wasps and fungal pathogens attack them.\n• Predators: spiders, crickets, lady beetles.";
                break;

            case "brown-planthopper":
                PestNameLabel.Text = "Brown Planthopper";
                PestImage.Source = "brown_planthopper.png";
                ScientificNameLabel.Text = "Nilaparvata lugens";
                CommonNamesLabel.Text = "Kayumangging hanip, kayumangging ngusong kabayo (Tagalog)";
                DescriptionLabel.Text = "Adults are 2.5–3.0 mm long, with or without wings. Hairless legs; hind leg has a large mobile outgrowth.";
                WhereToFindLabel.Text = "Thrives in irrigated and rainfed fields. Rare in upland rice. Higher nitrogen, dense planting, and high humidity favor outbreaks.";
                DamageLabel.Text = "Sucks sap at tiller base, causing yellowing and hopperburn. Transmits grassy, ragged, and wilted stunt viruses.";
                ManagementLabel.Text =
                    "Cultural:\n• 20x20 cm planting distance.\n• Keep seedbeds away from light sources.\n• Early-maturing varieties.\n• Split nitrogen applications.\n• Use potassium fertilizer.\n• Flood and dry alternately.\n\n" +
                    "Biological:\n• Encourage natural predators: spiders, dragonflies, lady beetles.\n• Parasitoids and fungi target eggs and nymphs.\n\n" +
                    "Chemical:\n• Apply insecticide only as last resort.\n• Don’t spray within 30 days after transplanting.";
                break;

            case "stem-borer":
                PestNameLabel.Text = "Stem Borer";
                PestImage.Source = "stem_borer.png";
                ScientificNameLabel.Text = "Scirpophaga incertulas (yellow); Chilo suppressalis (striped)";
                CommonNamesLabel.Text = "Uod ng tangkay (Tagalog)";
                DescriptionLabel.Text = "Larvae bore into stems, causing deadhearts (young plants) and whiteheads (mature plants).";
                WhereToFindLabel.Text = "Common in all rice areas in the Philippines, especially irrigated lowland.";
                DamageLabel.Text = "Reduces yield due to whiteheads and deadhearts.";
                ManagementLabel.Text =
                    "Cultural:\n• Synchronize planting.\n• Destroy stubbles.\n• Flood fields post-harvest.\n\n" +
                    "Biological:\n• Egg parasitoids (Trichogramma).\n• Predators like spiders and beetles.\n\n" +
                    "Chemical:\n• Use light traps; avoid unnecessary spraying.";
                break;

            case "whorl-maggot":
                PestNameLabel.Text = "Whorl Maggot";
                PestImage.Source = "whorl_maggot.png";
                ScientificNameLabel.Text = "Hydrellia philippina";
                CommonNamesLabel.Text = "Uod ng pilipit (Tagalog)";
                DescriptionLabel.Text = "Larvae feed on leaf whorls, causing folded or distorted leaves.";
                WhereToFindLabel.Text = "Prefers flooded rice fields, especially in seedling to early tillering stages.";
                DamageLabel.Text = "Distorted leaves reduce photosynthesis and vigor.";
                ManagementLabel.Text =
                    "Cultural:\n• Maintain proper water depth.\n• Ensure vigorous crop growth.\n\n" +
                    "Biological:\n• Encourage natural predators.\n\n" +
                    "Chemical:\n• Insecticide use is rarely needed.";
                break;

            case "leaf-folder":
                PestNameLabel.Text = "Leaf Folder";
                PestImage.Source = "leaf_folder.png";
                ScientificNameLabel.Text = "Cnaphalocrocis medinalis";
                CommonNamesLabel.Text = "Tiklop-dahon (Tagalog)";
                DescriptionLabel.Text = "Larvae fold leaves and feed inside, scraping green tissues.";
                WhereToFindLabel.Text = "Present throughout the country. Prefers wet season crops.";
                DamageLabel.Text = "Reduces photosynthesis by damaging leaves.";
                ManagementLabel.Text =
                    "Cultural:\n• Clip and destroy damaged leaves.\n• Use resistant varieties.\n\n" +
                    "Biological:\n• Encourage spiders and wasps.\n\n" +
                    "Chemical:\n• Use insecticide only during severe outbreaks.";
                break;

            case "green-leafhopper":
                PestNameLabel.Text = "Green Leafhopper";
                PestImage.Source = "green_leafhopper.png";
                ScientificNameLabel.Text = "Nephotettix virescens";
                CommonNamesLabel.Text = "Berdeng hanip (Tagalog)";
                DescriptionLabel.Text = "Small green insects that transmit tungro virus.";
                WhereToFindLabel.Text = "Common in irrigated and lowland rice areas.";
                DamageLabel.Text = "Transmits rice tungro virus. Sucks plant sap.";
                ManagementLabel.Text =
                    "Cultural:\n• Synchronize planting.\n• Destroy alternate hosts.\n\n" +
                    "Biological:\n• Encourage predators like spiders and frogs.\n\n" +
                    "Chemical:\n• Use resistant varieties and spray selectively if needed.";
                break;

            default:
                PestNameLabel.Text = "Unknown Pest";
                PestImage.Source = "";
                ScientificNameLabel.Text = "";
                CommonNamesLabel.Text = "";
                DescriptionLabel.Text = "No information available for this pest.";
                WhereToFindLabel.Text = "";
                DamageLabel.Text = "";
                ManagementLabel.Text = "";
                break;
        }
    }
}
