using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Weatherman.Common;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using System.Device.Location;
using Weatherman.API;
using Weatherman.API.Geocoding;
using Windows.Devices.Geolocation;

namespace Weatherman
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool isCurrentLoaded = false;
        bool isGeocodingLoaded = false;

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

                this.prgLoading.Visibility = System.Windows.Visibility.Collapsed;
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
            isCurrentLoaded = false;
            isGeocodingLoaded = false;

            LoadData(e.Position);
        }

        private void LoadData(GeoPosition<System.Device.Location.GeoCoordinate> position)
        {
            this.prgLoading.Visibility = System.Windows.Visibility.Visible;

            double latitude = position.Location.Latitude;
            double longitude = position.Location.Longitude;

            // seattle
            //double latitude = 47.6216;
            //double longitude = -122.3330;

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
                    this.txtNextHour.Text = FormatMinutelyText(result.minutely.summary);
                    this.txtNext24Hours.Text = FormatHourlyText(result.hourly.summary);

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

                    isCurrentLoaded = true;

                    if (isCurrentLoaded &&
                        isGeocodingLoaded)
                    {
                        ToggleLoadingText();

                        this.prgLoading.Visibility = System.Windows.Visibility.Collapsed;
                    }
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

                    isGeocodingLoaded = true;

                    if (isCurrentLoaded &&
                        isGeocodingLoaded)
                    {
                        ToggleLoadingText();

                        this.prgLoading.Visibility = System.Windows.Visibility.Collapsed;
                    }
                });
            });
        }

        private string FormatMinutelyText(string value)
        {
            string result = value.Replace("min.", "minutes").Trim();
            result = result.Replace(" 1 minutes", " 1 minute");

            if (result.EndsWith(".") == false)
                result = result + ".";

            return result;
        }

        private string FormatHourlyText(string value)
        {
            string result = value.Replace("min.", "minutes").Trim();
            result = result.Replace(" 1 minutes", " 1 minute");

            if (result.EndsWith(".") == false) 
                result = result + ".";

            return result;
        }

        private void ToggleLoadingText()
        {
            this.txtCurrentLoading.Visibility = System.Windows.Visibility.Collapsed;
            this.txtForecastLoading.Visibility = System.Windows.Visibility.Collapsed;

            this.stkCurrent.Visibility = System.Windows.Visibility.Visible;
            this.stkForecast.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (this.prgLoading.Visibility == System.Windows.Visibility.Visible) return;

            isCurrentLoaded = false;
            isGeocodingLoaded = false;

            LoadData(locationService.Position);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            if (this.prgLoading.Visibility == System.Windows.Visibility.Visible) return;

            SmartDispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
            });
        }
    }
}