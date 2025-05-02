using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;
using System.Diagnostics;

namespace SAA;

public partial class MainPage : ContentPage
{
    private string[] _labels = new[]
    {
        "rice leaf roller", "rice leaf caterpillar", "paddy stem maggot", "asiatic rice borer",
        "yellow rice borer", "rice gall midge", "brown plant hopper", "rice stem fly",
        "rice water weevil", "rice leaf hopper", "rice shell pest", "thrips"
    };

    private InferenceSession _session;

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
            string fileName = "model.onnx";
            string destinationPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            if (!File.Exists(destinationPath))
            {
                using var assetStream = Android.App.Application.Context.Assets.Open(fileName);
                using var fileStream = File.Create(destinationPath);
                assetStream.CopyTo(fileStream);
                Console.WriteLine($"✅ Model copied to: {destinationPath}");
            }
            else
            {
                Console.WriteLine("✅ Model already exists in AppDataDirectory.");
            }

            _session = new InferenceSession(destinationPath);
            Console.WriteLine("✅ ONNX session initialized.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading ONNX model: {ex.Message}");
        }
#endif
    }

    private async void OnCapturePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo == null)
                return;

            var filePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(filePath))
                await stream.CopyToAsync(newStream);

            CapturedImage.Source = ImageSource.FromFile(filePath);

            var inputTensor = PreprocessImage(filePath);
            if (inputTensor == null)
                return;

            if (_session == null)
            {
                Console.WriteLine("❌ Inference session is not initialized.");
                return;
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            using var results = _session.Run(inputs);
            var outputTensor = results.FirstOrDefault(r => r.Name == "sequential_1")?.AsTensor<float>();
            if (outputTensor == null)
                return;

            var scores = Softmax(outputTensor.ToArray());
            int predictedIndex = Array.IndexOf(scores, scores.Max());
            string predictedLabel = _labels[predictedIndex];
            float confidence = scores[predictedIndex] * 100f;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                PredictionLabel.Text = $"Prediction: {predictedLabel}\nConfidence: {confidence:F2}%";
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private DenseTensor<float> PreprocessImage(string imagePath)
    {
        try
        {
            using var stream = File.OpenRead(imagePath);
            var bitmap = SKBitmap.Decode(stream);

            if (bitmap == null)
                return null;

            var resized = bitmap.Resize(new SKImageInfo(128, 128), SKFilterQuality.High);
            if (resized == null)
                return null;

            var input = new DenseTensor<float>(new[] { 1, 128, 128, 3 });

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    var pixel = resized.GetPixel(x, y);
                    input[0, y, x, 0] = pixel.Red / 255f;
                    input[0, y, x, 1] = pixel.Green / 255f;
                    input[0, y, x, 2] = pixel.Blue / 255f;
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

    private float[] Softmax(float[] values)
    {
        float maxVal = values.Max();
        var exp = values.Select(v => Math.Exp(v - maxVal));
        double sumExp = exp.Sum();
        return exp.Select(v => (float)(v / sumExp)).ToArray();
    }
}
