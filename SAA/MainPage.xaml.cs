﻿using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using MySql.Data.MySqlClient;
using SkiaSharp;
using SQLite;
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

    private bool _locationReady = false;


    public MainPage()
    {
        InitializeComponent();
        InitDetectionDatabase();
        LoadModelFromAssets();
        _ = EnsureLocationAsync();
        Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
    }

    private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            Task.Run(async () => await TrySyncUnsyncedDetections());
        }
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
        ShowLoading();
        var loadingStart = DateTime.UtcNow;

        try
        {
            var location = await GetLocationAsync();
            if (location == null)
            {
                await EnsureMinimumLoadingTime(loadingStart);
                HideLoading();
                return;
            }

            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo == null)
            {
                CapturedImage.Source = null;
                await EnsureMinimumLoadingTime(loadingStart);
                HideLoading();
                return;
            }

            var filePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(filePath))
                await stream.CopyToAsync(newStream);

            await ProcessImage(filePath);

            // ✅ Show location alert after capture and processing
           // await DisplayAlert("📍 Location Captured", $"Latitude: {location.Latitude}\nLongitude: {location.Longitude}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            await EnsureMinimumLoadingTime(loadingStart);
            HideLoading();
        }
    }


    private void ShowLoading()
    {
        LoadingOverlay.IsVisible = true;
        LoadingIndicator.IsRunning = true;
    }

    private void HideLoading()
    {
        LoadingIndicator.IsRunning = false;
        LoadingOverlay.IsVisible = false;
    }

    private async Task EnsureMinimumLoadingTime(DateTime startTime)
    {
        var elapsed = DateTime.UtcNow - startTime;
        var remaining = TimeSpan.FromMilliseconds(500) - elapsed;

        if (remaining > TimeSpan.Zero)
        {
            await Task.Delay(remaining);
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

        // Save each detection to the local database
        var location = await GetLocationAsync();
        foreach (var detection in detections)
        {
            var log = new DetectionLog
            {
                PestName = detection.Label,
                Confidence = detection.Confidence,
                DetectionTime = DateTime.Now,
                Latitude = location?.Latitude ?? 0,
                Longitude = location?.Longitude ?? 0,
                ImagePath = filePath,
                IsSynced = false
            };

            _detectionDb.Insert(log);
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
        await TrySyncUnsyncedDetections();
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

    private async Task<bool> EnsureLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                bool openSettings = await DisplayAlert(
                    "Location Required",
                    "This app requires location access to continue. Please enable location services.",
                    "Open Location",
                    "Cancel");

                if (openSettings)
                {
#if ANDROID
                var intent = new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                intent.SetFlags(Android.Content.ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
#endif
                }
                else
                {
                    // Close app
#if ANDROID
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
                }

                return false;
            }

            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
                location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

            if (location == null)
            {
                await DisplayAlert("Location Required", "Unable to get current location. Please enable location services.", "OK");
                return false;
            }

            Console.WriteLine($"Location: Lat {location.Latitude}, Lon {location.Longitude}");
            return true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Location Error", ex.Message, "OK");
            return false;
        }
    }


    private async Task<Location?> GetLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Location Required", "This app requires location access to continue.", "OK");
                return null;
            }

            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
                location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

            return location;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Location Error", ex.Message, "OK");
            return null;
        }
    }
    public class DetectionLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PestName { get; set; }
        public double? Confidence { get; set; }  
        public DateTime DetectionTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImagePath { get; set; }
        public bool IsSynced { get; set; }
    }

    private SQLiteConnection _detectionDb;

    private void InitDetectionDatabase()
    {
        try
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "detections.db");
            _detectionDb = new SQLiteConnection(dbPath);
            _detectionDb.CreateTable<DetectionLog>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Detection DB Init Error: {ex.Message}");
        }
    }
    public async Task TrySyncUnsyncedDetections()
    {
        try
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
                return;

            var unsyncedDetections = _detectionDb.Table<DetectionLog>().Where(d => !d.IsSynced).ToList();

            foreach (var detection in unsyncedDetections)
            {
                try
                {
                    using var conn = new MySqlConnection("server=192.168.100.29;uid=root;pwd=;database=saa_db;");
                    await conn.OpenAsync();

                    var cmd = new MySqlCommand(
                        "INSERT INTO detection_logs (pest_name, confidence, detection_time, latitude, longitude, image_path) " +
                        "VALUES (@pestName, @confidence, @detectionTime, @latitude, @longitude, @imagePath)", conn);

                    cmd.Parameters.AddWithValue("@pestName", detection.PestName);
                    cmd.Parameters.AddWithValue("@confidence", detection.Confidence);
                    cmd.Parameters.AddWithValue("@detectionTime", detection.DetectionTime);
                    cmd.Parameters.AddWithValue("@latitude", detection.Latitude);
                    cmd.Parameters.AddWithValue("@longitude", detection.Longitude);
                    cmd.Parameters.AddWithValue("@imagePath", detection.ImagePath);

                    await cmd.ExecuteNonQueryAsync();

                    detection.IsSynced = true;
                    _detectionDb.Update(detection);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Sync Error", $"Failed to sync detection {detection.Id}: {ex.Message}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Sync Error", $"General sync error: {ex.Message}", "OK");
        }
    }
}
