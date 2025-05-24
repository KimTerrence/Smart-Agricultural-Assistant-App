using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System.Diagnostics;

namespace SAA;

public partial class MainPage : ContentPage
{
    private string[] _labels = new[]
    {
        "brown-planthopper", "green-leafhopper", "leaf-folder",
        "rice-bug", "stem-borer", "whorl-maggot"
    };

    private InferenceSession _session;

    private Dictionary<string, SKColor> _labelColors = new()
    {
        ["brown-planthopper"] = SKColors.Red,
        ["green-leafhopper"] = SKColors.Green,
        ["leaf-folder"] = SKColors.Blue,
        ["rice-bug"] = SKColors.Orange,
        ["stem-borer"] = SKColors.Purple,
        ["whorl-maggot"] = SKColors.Teal
    };

    public MainPage()
    {
        InitializeComponent();
        LoadModelFromAssets();
    }

    private void LoadModelFromAssets()
    {
#if ANDROID
        try
        {
            string fileName = "RicePest.onnx";
            string destinationPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            if (!File.Exists(destinationPath))
            {
                using var assetStream = Android.App.Application.Context.Assets.Open(fileName);
                using var fileStream = File.Create(destinationPath);
                assetStream.CopyTo(fileStream);
            }

            _session = new InferenceSession(destinationPath);
        }
        catch
        {
            DisplayAlert("", $"❌ Detection Unavailable", "OK");
        }
#endif
    }

    private async void OnCapturePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo == null)
            {
                CapturedImage.Source = ImageSource.FromFile("");
                PredictionList.Children.Clear();
                return;
            }

            var filePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(filePath))
                await stream.CopyToAsync(newStream);

            await ProcessImage(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBrowseImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select an image"
            });

            if (result == null)
            {
                CapturedImage.Source = "";
                PredictionList.Children.Clear();
                return;
            }

            await ProcessImage(result.FullPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task ProcessImage(string filePath)
    {
        CapturedImage.Source = ImageSource.FromFile(filePath);

        var inputTensor = PreprocessImage(filePath);
        if (inputTensor == null || _session == null)
            return;

        using var originalImage = SKBitmap.Decode(filePath);
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("images", inputTensor)
        };

        using var results = _session.Run(inputs);
        var outputTensor = results.FirstOrDefault(r => r.Name == "output0")?.AsTensor<float>();
        if (outputTensor == null) return;

        var rawDetections = ParseDetections(outputTensor, 416, 416, originalImage.Width, originalImage.Height, 0.5f);
        var detections = NonMaximumSuppression(rawDetections, 0.5f);

        if (detections.Count == 0)
        {
            PredictionList.Children.Clear();
            await DisplayAlert("", "No pests detected.", "OK");
            return;
        }

        var imageWithBoxes = DrawBoundingBoxes(originalImage, detections);
        using var image = SKImage.FromBitmap(imageWithBoxes);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        byte[] imageBytes = data.ToArray(); // Copy data to prevent stream disposal issues

        CapturedImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));

        var uniquePests = detections.Select(d => d.Label).Distinct();

        PredictionList.Children.Clear();
        PredictionList.Children.Add(new Label
        {
            Text = "Detected Pests:",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.DarkGreen
        });

        foreach (var pest in uniquePests)
        {
            var color = _labelColors.TryGetValue(pest, out var skColor)
                ? Color.FromRgba(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha)
                : Colors.Black;

            var frame = new Frame
            {
                CornerRadius = 10,
                BackgroundColor = color,
                Padding = new Thickness(10, 5),
                Margin = new Thickness(5),
                HeightRequest = 50,
                VerticalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = pest,
                    FontSize = 14,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) =>
            {
                await Navigation.PushAsync(new PestDetailPage(pest));
            };

            frame.GestureRecognizers.Add(tap);
            PredictionList.Children.Add(frame);
        }
    }

    private DenseTensor<float> PreprocessImage(string imagePath)
    {
        try
        {
            using var stream = File.OpenRead(imagePath);
            var bitmap = SKBitmap.Decode(stream);
            var resized = bitmap.Resize(new SKImageInfo(416, 416), SKFilterQuality.High);

            var input = new DenseTensor<float>(new[] { 1, 3, 416, 416 });
            for (int y = 0; y < 416; y++)
            {
                for (int x = 0; x < 416; x++)
                {
                    var pixel = resized.GetPixel(x, y);
                    input[0, 0, y, x] = pixel.Red / 255f;
                    input[0, 1, y, x] = pixel.Green / 255f;
                    input[0, 2, y, x] = pixel.Blue / 255f;
                }
            }
            return input;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Preprocessing error: {ex.Message}");
            return null;
        }
    }

    private List<PredictionResult> ParseDetections(
        Tensor<float> output, int inputWidth, int inputHeight,
        int originalWidth, int originalHeight, float confidenceThreshold = 0.4f)
    {
        var results = new List<PredictionResult>();
        int numDetections = output.Dimensions[1];
        int numClasses = _labels.Length;

        float xScale = (float)originalWidth / inputWidth;
        float yScale = (float)originalHeight / inputHeight;

        for (int i = 0; i < numDetections; i++)
        {
            float objConf = output[0, i, 4];
            if (objConf < confidenceThreshold)
                continue;

            var classScores = new float[numClasses];
            for (int j = 0; j < numClasses; j++)
                classScores[j] = output[0, i, 5 + j];

            int classId = Array.IndexOf(classScores, classScores.Max());
            float classConf = classScores[classId] * objConf;
            if (classConf < confidenceThreshold)
                continue;

            var result = new PredictionResult
            {
                X = output[0, i, 0] * xScale,
                Y = output[0, i, 1] * yScale,
                Width = output[0, i, 2] * xScale,
                Height = output[0, i, 3] * yScale,
                Confidence = classConf,
                Label = _labels[classId]
            };
            results.Add(result);
        }

        return results;
    }

    private SKBitmap DrawBoundingBoxes(SKBitmap image, List<PredictionResult> boxes)
    {
        var canvas = new SKCanvas(image);

        foreach (var box in boxes)
        {
            float x = box.X - box.Width / 2;
            float y = box.Y - box.Height / 2;
            float w = box.Width;
            float h = box.Height;

            var rect = new SKRect(x, y, x + w, y + h);
            SKColor color = _labelColors.TryGetValue(box.Label, out var c) ? c : SKColors.White;

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = color,
                StrokeWidth = 5
            };

            var textPaint = new SKPaint
            {
                Color = color,
                TextSize = 20,
                IsAntialias = true,
                IsStroke = false,
                Typeface = SKTypeface.Default
            };

            canvas.DrawRect(rect, paint);
            canvas.DrawText($"{box.Label} ({box.Confidence:P1})", x, y - 5, textPaint);
        }

        return image;
    }

    private List<PredictionResult> NonMaximumSuppression(List<PredictionResult> detections, float iouThreshold)
    {
        var results = new List<PredictionResult>();
        var sorted = detections.OrderByDescending(d => d.Confidence).ToList();

        while (sorted.Count > 0)
        {
            var best = sorted[0];
            results.Add(best);
            sorted.RemoveAt(0);

            sorted = sorted.Where(d => IoU(best, d) < iouThreshold).ToList();
        }

        return results;
    }

    private float IoU(PredictionResult a, PredictionResult b)
    {
        float x1 = Math.Max(a.X - a.Width / 2, b.X - b.Width / 2);
        float y1 = Math.Max(a.Y - a.Height / 2, b.Y - b.Height / 2);
        float x2 = Math.Min(a.X + a.Width / 2, b.X + b.Width / 2);
        float y2 = Math.Min(a.Y + a.Height / 2, b.Y + b.Height / 2);

        float interWidth = Math.Max(0, x2 - x1);
        float interHeight = Math.Max(0, y2 - y1);
        float interArea = interWidth * interHeight;

        float areaA = a.Width * a.Height;
        float areaB = b.Width * b.Height;

        float union = areaA + areaB - interArea;
        return interArea / union;
    }

    public class PredictionResult
    {
        public float X, Y, Width, Height, Confidence;
        public required string Label;
    }

    private async void OnBrowsePestClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PestListPage());
    }

}
