﻿using ClassLibrary;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
namespace Client
{
    /// <summary>
    /// Логика взаимодействия для VideoPage.xaml
    /// </summary>
    public partial class VideoPage : Page
    {
        public VideoPage(MainWindow window)
        {
            InitializeComponent();
            _window = window;
            Canvas canvas = FindName("canvas2") as Canvas;
            canvas.Children.Add(rectangleContainer);

            ComboBoxForResponse.ItemsSource = OpenVideos;
            localDrawer = new Drawer(rectangleContainer, VideoImage, _window);
        }
        
        MainWindow _window;

        private readonly Canvas rectangleContainer = new();
        public readonly Drawer localDrawer;

        public ObservableCollection<string> OpenVideos { get; } = new ObservableCollection<string>();
        private List<VideoController> _videoControllers = [];
        public VideoController currentVideoController;

        private string[] files;

        private void MediaPlayButton_Click(object sender, RoutedEventArgs e)
        {
            currentVideoController?.Play();
        }

        private void MediaPauseButton_Click(object sender, RoutedEventArgs e)
        {
            currentVideoController?.Pause();
        }

        private void MediaStopButton_Click(object sender, RoutedEventArgs e)
        {
            currentVideoController?.Stop();
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            currentVideoController?.Rewind();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            currentVideoController?.NextFrame();
        }
        private void ShowInfoButton_Click(object sender, RoutedEventArgs e)
        {
            currentVideoController?.ShowInfo();
        }
        private void MediaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((Slider)sender).SelectionEnd = e.NewValue;
            currentVideoController?.GetSliderValue(e.NewValue);
        }
        private void UploadMediaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                files = FileHandler.OpenFile(FileTypes.Media);
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        currentVideoController = new(file, VideoImage, MediaSlider, _window);
                        _videoControllers.Add(currentVideoController);
                        OpenVideos.Add($"{_videoControllers.Count}. {currentVideoController.shortName}");
                    }
                    ComboBoxForResponse.SelectedIndex = 0;
                    ComboBoxForResponse.UpdateLayout();


                    MediaSlider.Value = 0;
                }
            }
            catch (Exception ex)
            {
                Logger.LogByTemplate(LogEventLevel.Error, ex, note: "Media file opening error.");
                MessageBox.Show($"Media file opening error: {ex.Message}");
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentVideoController.GetProcessedVideo();
            }
            catch { }

        }

        private async void HealthCheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MainWindow.apiClient.CheckHealthAsync($"{ConnectionWindow.ConnectionUri}"))
            {
                MessageBox.Show("Yes");
            }
            else
            {
                MessageBox.Show("No");
            }
        }
        private void VideoBox_SourceUpdated(object sender, RoutedEventArgs e)
        {
            localDrawer.CalculateScale();
            localDrawer.ClearRectangles();
            Logger.LogByTemplate(LogEventLevel.Information, note: $"Image uploaded.");
        }
        private void ComboBoxForResponse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                currentVideoController.IsPaused = true;
                currentVideoController = _videoControllers[comboBox.SelectedIndex];
                currentVideoController.SetFirstFrame();
            }
            else
            {
                currentVideoController = null;
                VideoImage.Source = null;
                _window.activyVideoPage.localDrawer.ClearRectangles();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxForResponse.Items.Count == 0) return;
            if (ComboBoxForResponse.Items.Count == 1)
            {
                ComboBoxForResponse.SelectedIndex = -1;
                _videoControllers.Clear();
                OpenVideos.Clear();
                return;
            }
            int selectedIndex = ComboBoxForResponse.SelectedIndex;
            ComboBoxForResponse.SelectedIndex = selectedIndex > 0 ? selectedIndex - 1 : 0;
            _videoControllers.RemoveAt(selectedIndex);
            OpenVideos.RemoveAt(selectedIndex);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            currentVideoController?.SaveFullVideo();
            MessageBox.Show("Done");
        }
    }
}
