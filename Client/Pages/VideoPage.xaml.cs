﻿using ClassLibrary;
using Client;
using Serilog.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace SocketClient
{
    /// <summary>
    /// Логика взаимодействия для VideoPage.xaml
    /// </summary>
    public partial class VideoPage : Page
    {
        private readonly Canvas rectangleContainer = new();
        #region Constructor
        public VideoPage(MainWindow window)
        {
            InitializeComponent();
            _window = window;
            Canvas canvas = FindName("canvas2") as Canvas;
            canvas.Children.Add(rectangleContainer);
        }
        #endregion
        #region Attributes
        private string filepath;
        VideoController videoController;

        MainWindow _window;

        private double originalWidth;
        private double originalHeight;
        private double scaleX;
        private double scaleY;
        #endregion

        #region Media Control Methods
        private void MediaPlayButton_Click(object sender, RoutedEventArgs e)
        {
            videoController.Play();
        }

        private void MediaPauseButton_Click(object sender, RoutedEventArgs e)
        {
            videoController.Pause();
        }

        private void MediaStopButton_Click(object sender, RoutedEventArgs e)
        {
            videoController.Stop();
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            videoController.Rewind();
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            videoController.NextFrame();
        }
        private void MediaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((Slider)sender).SelectionEnd = e.NewValue;
            if (videoController != null)
            {
                videoController.GetSliderValue(e.NewValue);
            }
        }

        private void UploadMediaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                filepath = FileHandler.OpenFile("Media");
                if (filepath != null)
                {
                    videoController = new(filepath, VideoImage, MediaSlider, _window);
                    MediaSlider.Value = 0;
                }

            }
            catch (Exception ex)
            {
                Logger.LogByTemplate(LogEventLevel.Error, ex, note: "Media file openning error.");
                MessageBox.Show($"Media file opening error: {ex.Message}");
            }
        }
        #endregion
        #region Service Communication Button Methods
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxForResponce.Items.Add(SqlCore.ReturnLogEventAsString(MainWindow.connectionString));
        }

        public void DrawBoundingBoxes(List<ObjectOnPhoto> aircraftObjects)
        {
            foreach (var obj in aircraftObjects)
            {
                double xtl = obj.XTopLeftCorner;
                double ytl = obj.YTopLeftCorner;
                double xbr = obj.XBottonRigtCorner;
                double ybr = obj.YBottonRigtCorner;
                string name = obj.Class_name;
                DrawBoundingBox(xtl, ytl, xbr, ybr, name);
            }
            Logger.LogByTemplate(LogEventLevel.Information, note: $"{aircraftObjects.Count} borders of objects have been drawn.");
        }
        private void DrawBoundingBox(double xTopLeft, double yTopLeft, double xBottomRight, double yBottomRight, string name)
        {
            CalculateScale();
            Rectangle boundingBox = new Rectangle();

            double scaledXTopLeft = xTopLeft * scaleX;
            double scaledYTopLeft = yTopLeft * scaleY;
            double scaledWidth = (xBottomRight - xTopLeft) * scaleX;
            double scaledHeight = (yBottomRight - yTopLeft) * scaleY;

            boundingBox.Width = scaledWidth;
            boundingBox.Height = scaledHeight;
            Canvas canvas = FindName("canvas2") as Canvas;
            if (canvas != null)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = name;
                textBlock.FontSize = 16; 
                textBlock.Foreground = Brushes.Red; 
                Canvas.SetLeft(textBlock, scaledXTopLeft);
                Canvas.SetTop(textBlock, scaledYTopLeft);
                rectangleContainer.Children.Add(textBlock);
                Canvas.SetLeft(boundingBox, scaledXTopLeft);
                Canvas.SetTop(boundingBox, scaledYTopLeft);
                Canvas.SetZIndex(boundingBox, int.MaxValue);

                boundingBox.Stroke = Brushes.Red;
                boundingBox.StrokeThickness = 2;
                boundingBox.Fill = Brushes.Transparent;

                rectangleContainer.Children.Add(boundingBox);
            }
        }
        public void ClearRectangles()
        {
            rectangleContainer.Children.Clear();
        }
        private void VideoBox_SourceUpdated(object sender, RoutedEventArgs e)
        {
            CalculateScale();
            ClearRectangles();
            Logger.LogByTemplate(LogEventLevel.Information, note: $"Image uploaded.");
        }
        private void CalculateScale()

        {
            if (VideoImage.Source is BitmapSource)
            {
                BitmapSource bitmapSource = (BitmapSource)VideoImage.Source;
                double currentWidth = VideoImage.ActualWidth;
                double currentHeight = VideoImage.ActualHeight;

                originalWidth = bitmapSource.Width;
                originalHeight = bitmapSource.Height;

                scaleX = currentWidth / originalWidth;
                scaleY = currentHeight / originalHeight;
            }
            Logger.LogByTemplate(LogEventLevel.Debug, note: "The image scale is calculated");
        }
        #endregion

        private async void HealthCheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MainWindow.apiClient.CheckHealthAsync($"{ConnectionWindow.ConnectionUri}health"))
            {
                MessageBox.Show("Yes");
            }
        }
    }
}
