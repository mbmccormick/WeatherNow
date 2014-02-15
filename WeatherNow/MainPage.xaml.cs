﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using WeatherNow.Common;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using System.Device.Location;
using WeatherNow.API;
using WeatherNow.API.Geocoding;
using Windows.Devices.Geolocation;

namespace WeatherNow
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher locationService;

        public MainPage()
        {
            InitializeComponent();

            App.UnhandledExceptionHandled += new EventHandler<ApplicationUnhandledExceptionEventArgs>(App_UnhandledExceptionHandled);

            this.BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            ApplicationBarIconButton refresh = new ApplicationBarIconButton();
            refresh.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Refresh.png", UriKind.RelativeOrAbsolute);
            refresh.Text = "refresh";
            refresh.Click += btnRefresh_Click;

            ApplicationBar.Buttons.Add(refresh);

            ApplicationBarMenuItem about = new ApplicationBarMenuItem();
            about.Text = "about";
            about.Click += mnuAbout_Click;

            ApplicationBar.MenuItems.Add(about);
        }

        private void App_UnhandledExceptionHandled(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                ToggleLoadingText();

                GlobalLoading.Instance.IsLoading = false;
            });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.IsNavigationInitiator == false)
            {
                LittleWatson.CheckForPreviousException(true);
            }

            locationService = new GeoCoordinateWatcher();
            locationService.PositionChanged += locationService_PositionChanged;

            locationService.Start();
        }

        private void locationService_PositionChanged(object sender, GeoPositionChangedEventArgs<System.Device.Location.GeoCoordinate> e)
        {
            LoadData(e.Position);
        }

        private void LoadData(GeoPosition<System.Device.Location.GeoCoordinate> position)
        {
            GlobalLoading.Instance.IsLoading = true;

            double latitude = position.Location.Latitude;
            double longitude = position.Location.Longitude;

            // san francisco
            //double latitude = 37.7882;
            //double longitude = -122.4131;

            // los angeles
            //double latitude = 34.0535;
            //double longitude = -118.2453;

            // atlanta
            //double latitude = 33.7483;
            //double longitude = -84.3911;

            ForecastIORequest request = new ForecastIORequest("29b5b5502df6a3e0c0446fb72ded0a97", latitude, longitude, Unit.auto);
            request.Get((result) =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    this.txtTemperature.Text = Convert.ToInt32(result.currently.temperature) + "°";
                    this.txtCondition.Text = FormatText(result.currently.summary) + ", feels like " + Convert.ToInt32(result.currently.apparentTemperature) + "°.";
                    this.txtDescription.Text = result.minutely.summary + " " + result.hourly.summary;

                    switch (result.currently.icon)
                    {
                        case "clear-day":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 222, 215, 20));
                            break;
                        case "clear-night":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 133, 133, 133));
                            break;
                        case "rain":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 141, 196, 196));
                            break;
                        case "snow":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
                            break;
                        case "sleet":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
                            break;
                        case "wind":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 76, 224, 81));
                            break;
                        case "fog":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 255, 139, 89));
                            break;
                        case "cloudy":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 64, 153, 255));
                            break;
                        case "partly-cloudy-day":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 64, 153, 255));
                            break;
                        case "partly-cloudy-night":
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 133, 133, 133));
                            break;
                        default:
                            this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 64, 153, 255));
                            break;
                    }

                    this.lstForecast.ItemsSource = result.hourly.data;

                    ToggleLoadingText();

                    GlobalLoading.Instance.IsLoading = false;
                });
            });

            GeocodeClient client = new GeocodeClient();
            client.GeocodeAddress(latitude + ", " + longitude, false, (result) =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    foreach (var item in result.Results)
                    {
                        if (item.Types.Contains("locality") == true)
                            this.pivLayout.Title = item.FormattedAddress.Replace(", USA", "").ToUpper();
                    }
                });
            });
        }

        private string FormatText(string value)
        {
            string result = value.ToLower();
            result = result.Substring(0, 1).ToUpper() + result.Substring(1);

            return result;
        }

        private void ToggleLoadingText()
        {
            this.txtLoading.Visibility = System.Windows.Visibility.Collapsed;
            this.stkDefault.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData(locationService.Position);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
            });
        }
    }
}