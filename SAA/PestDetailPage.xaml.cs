namespace SAA;

public partial class PestDetailPage : ContentPage
{
    public PestDetailPage(string pestName)
    {
        InitializeComponent();

        PestNameLabel.Text = pestName;

        // Fill labels based on the pest
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
                CommonNamesLabel.Text = "Atangya (Tagalog); dangaw (Ilocano)";
                DescriptionLabel.Text = "Young nymphs are green while adults are greenish‐brown. " +
                    "When the temperature is high, and the insects are not feeding, they camouflage themselves " +
                    "on the plant by taking up a particular posture. Adults live 30‐50 days, with reports of 110‐115 days " +
                    "when reared individually. Mating starts 7‐14 days after becoming adult. Pre‐oviposition period is 3‐4 days. " +
                    "Eggs hatch in 5‐8 days. A female lays 200‐300 eggs in batches of 10‐20. Females rest on grassy areas " +
                    "and at the base of plants during sunlight.";
                WhereToFindLabel.Text = "Found in all environments but more prevalent in rainfed wetland or upland rice. " +
                    "Susceptible growth stages are from flowering to milky stage.";
                DamageLabel.Text = "Adults and nymphs appear in the young crop with early rains. They suck sap from developing grains at milky stage. " +
                    "Attack causes discolored or shriveled grains, off‐smell of raw and cooked rice, and off‐flavor of straws.";
                ManagementLabel.Text = "Cultural:\n" +
                    "• Eliminate grassy weeds from fields and surroundings to reduce egg-laying habitats.\n" +
                    "• Avoid staggered planting to break continuous food supply.\n" +
                    "• Smoke fields, use sticky traps, handpicking, and netting to reduce bugs.\n" +
                    "• Use attractants with bad odor to gather bugs for removal.\n" +
                    "• Use resistant or mechanically resistant rice varieties.\n\n" +
                    "Biological:\n" +
                    "• Small wasps and grasshoppers kill eggs.\n" +
                    "• Fungal pathogens infect nymphs and adults.\n" +
                    "• Predators like spiders, crickets, and lady beetles feed on them.";
                break;


            case "brown-planthopper":
                PestNameLabel.Text = "Brown Planthopper";
                PestImage.Source = "rice_bug.png";
                ScientificNameLabel.Text = "Nilaparvata lugens";
                CommonNamesLabel.Text = "kayumangging hanip, kayumangging ngusong kabayo(Tagalog)";
                DescriptionLabel.Text = " Adults are 2.5-3.0 mm long, winged, or without wings. The legs are hairless and the hind leg has a large, mobile outgrowth.";
                WhereToFindLabel.Text = "Rainfed and irrigated wetland fields are preferred. It is rare in upland rice conditions. Direct-sown fields are more prone to heavy damage than transplanted fields. All plant growth stages are attacked, but the most susceptible growth stages are from early tillering to flowering. Increasing nitrogen levels, closer plant spacing, and higher alternative humidity increase their numbers.";
                DamageLabel.Text = "Adults and nymphs cause direct damage by sucking the sap at the base of the tillers. Plants turn yellow and dry up rapidly. Heavy infestation creates brown patches of dried plants known as hopperburn. They also transmit viral diseases: ragged stunt, grassy stunt and wilted stunt. Excreted honeydew on infested plants may also become a medium for sooty mold fungus.";
                ManagementLabel.Text = "cultural \n" + "20 days before transplanting \n" +
                    "•Observe the 20 cm x 20 cm planting distance. Dense planting increases number of planthoppers \n" +
                    "Seedbed areas must be as far as possible from light sources to discourage hopper attack and virus infection by virus-infected hoppers. \n" +
                    "Plant early-maturing varieties to create a rice-free period during the year. \n" +
                    "Use appropriate and balanced fertilization. High nitrogen use increases planthopper attack. Split nitrogen into three applications during crop growth to reduce BPH buildup. \n" +
                    "Increased potassium reduces planthopper susceptibility as cell walls get thicker because of greater silica uptake. \n" +
                    "Grow only two rice crops per year and use early-maturing varieties to reduce their continuous breeding. \n" +
                    "Plow under volunteer rations after harvest. \n" +
                    "Raise the level of irrigation water periodically to drown the eggs, which are deposited at the base of the tillers and in leaf sheaths. \n" +
                    " At tillering stage: \n" +
                    "Keep water level low enhances growth of useful organisms \n" +
                    "Keep water level low enhances growth of useful organisms \n" +
                    " At milk stage: \n" +
                    "Dry and flood the paddy alternately reduces their growth \n" +
                    "Use selective insecticide if level of pest infestation is very high to spare beneficial organisms. \n"  +
                    "\n" +
                    "Biological Control \n" +
                    "Avoid early application of pesticides or establish refuge areas to encourage buildup of useful organisms. \n" +
                    "Small wasps attack eggs \n" +
                    "Mirid bugs prey on eggs \n" +
                    "Dragonflies and damselflies prey on moving adults. Similarly, spiders, water bugs, and lady beetles prey on mobile stages (nymphs and adults). \n" +
                    "Dryinid kills nymphs \n" +
                    "Fungus kills nymphs and adults \n" +
                    "\n" +
                    "Chemical Control \n" +
                    "Apply insecticide as a last resort and its benefits should be weighed against the risk. \n" +
                    "Seek the advice of a crop protection specialist for guidance before applying insecticides \n" +
                    "Always read instructions \n" +
                    "Do not spray 30 days after transplanting or 40 days after seeding. Plants can recover from early damages by producing new leaves and tillers. Spraying prevents the early season movement and colonization of beneficial organisms."
                    ;
                break;

            case "stem-borer":
                PestNameLabel.Text = "Stem Borer";
                PestImage.Source = "";
                ScientificNameLabel.Text = "";
                CommonNamesLabel.Text = "";
                DescriptionLabel.Text = "No information available for this pest.";
                WhereToFindLabel.Text = "";
                DamageLabel.Text = "";
                ManagementLabel.Text = "";
                break;

            case "whorl-maggot":
                PestNameLabel.Text = "Whorl Maggot";
                PestImage.Source = "";
                ScientificNameLabel.Text = "";
                CommonNamesLabel.Text = "";
                DescriptionLabel.Text = "No information available for this pest.";
                WhereToFindLabel.Text = "";
                DamageLabel.Text = "";
                ManagementLabel.Text = "";
                break;

            case "leaf-folder":
                PestNameLabel.Text = "Leaf Folder";
                PestImage.Source = "";
                ScientificNameLabel.Text = "";
                CommonNamesLabel.Text = "";
                DescriptionLabel.Text = "No information available for this pest.";
                WhereToFindLabel.Text = "";
                DamageLabel.Text = "";
                ManagementLabel.Text = "";
                break;

            case "green-leafhopper":
                PestNameLabel.Text = "Green Leafhopper";
                PestImage.Source = "";
                ScientificNameLabel.Text = "";
                CommonNamesLabel.Text = "";
                DescriptionLabel.Text = "No information available for this pest.";
                WhereToFindLabel.Text = "";
                DamageLabel.Text = "";
                ManagementLabel.Text = "";
                break;

            default:
                PestNameLabel.Text = "";
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
