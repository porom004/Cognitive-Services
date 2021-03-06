﻿using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NotHotDog
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string _uri = "PREDICTION_ENDPOINT";
        private const string _key = "PREDICTION_KEY";
        private const string _name = "ITERATION_NAME";
        private Guid _id = Guid.Parse("PROJECT_ID");

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void OnSelectImageButtonClicked(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    // Show the image
                    var image = new BitmapImage();
                    await image.SetSourceAsync(stream);
                    LoadedImage.Source = image;
                    stream.Seek(0L);

                    try
                    {
                        Progress.IsActive = true;
                        Overlay.Visibility = Visibility.Visible;

                        // Submit the image to the Custom Vision Service
                        CustomVisionPredictionClient client = new CustomVisionPredictionClient()
                        {
                            ApiKey = _key,
                            Endpoint = _uri
                        };

                        var result = await client.ClassifyImageAsync(_id, _name, stream.AsStream());

                        Progress.IsActive = false;
                        Overlay.Visibility = Visibility.Collapsed;

                        // Show the result
                        var probability = result.Predictions.FirstOrDefault(x => x.TagName.ToLowerInvariant() == "hotdog").Probability;

                        if (probability > 0.90)
                        {
                            await new MessageDialog("It's a hot dog!").ShowAsync();
                        }
                        else
                        {
                            await new MessageDialog("Not a hot dog").ShowAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Progress.IsActive = false;
                        Overlay.Visibility = Visibility.Collapsed;

                        await new MessageDialog(ex.Message).ShowAsync();
                    }
                    finally
                    {
                        Progress.IsActive = false;
                        Overlay.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
