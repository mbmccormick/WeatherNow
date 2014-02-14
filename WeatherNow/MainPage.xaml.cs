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
using WeatherNow.Common;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using System.Device.Location;
using WeatherNow.API;

namespace WeatherNow
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher _watcher;

        private bool isLoaded = false;

        public MainPage()
        {
            InitializeComponent();

            App.UnhandledExceptionHandled += new EventHandler<ApplicationUnhandledExceptionEventArgs>(App_UnhandledExceptionHandled);

            _watcher = new GeoCoordinateWatcher();
            _watcher.Start();

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

                if (isLoaded == false)
                    LoadData();
            }
        }

        private void LoadData()
        {
            GlobalLoading.Instance.IsLoading = true;

            GeoCoordinate currentLocation = _watcher.Position.Location;

            ForecastIORequest request = new ForecastIORequest("29b5b5502df6a3e0c0446fb72ded0a97", currentLocation.Latitude, currentLocation.Longitude, Unit.auto);
            request.Get((result) =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    string temperatureUnit = result.flags.units == "us" ? "°F" : "°C";

                    this.txtCondition.Text = result.currently.summary;
                    this.txtTemperature.Text = Convert.ToInt32(result.currently.temperature) + " " + temperatureUnit;
                    this.txtDescription.Text = result.minutely.summary + " " + result.hourly.summary;

                    this.vbxClearDay.Visibility = System.Windows.Visibility.Collapsed;
                    this.vbxRain.Visibility = System.Windows.Visibility.Collapsed;
                    this.vbxSnow.Visibility = System.Windows.Visibility.Collapsed;
                    this.vbxSleet.Visibility = System.Windows.Visibility.Collapsed;
                    this.vbxPartlyCloudyDay.Visibility = System.Windows.Visibility.Collapsed;

                    switch (result.currently.icon)
                    {
                        case "clear-day":
                            this.vbxClearDay.Visibility = System.Windows.Visibility.Visible;
                            break;
                        case "rain":
                            this.vbxRain.Visibility = System.Windows.Visibility.Visible;
                            break;
                        case "snow":
                            this.vbxSnow.Visibility = System.Windows.Visibility.Visible;
                            break;
                        case "sleet":
                            this.vbxSleet.Visibility = System.Windows.Visibility.Visible;
                            break;
                        case "partly-cloudy-day":
                            this.vbxPartlyCloudyDay.Visibility = System.Windows.Visibility.Visible;
                            break;
                        default:
                            break;
                    }

                    this.LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 36, 171, 100));

                    isLoaded = true;

                    ToggleLoadingText();

                    GlobalLoading.Instance.IsLoading = false;
                });
            });
        }

        private void ToggleLoadingText()
        {
            this.txtLoading.Visibility = System.Windows.Visibility.Collapsed;
            this.stkDefault.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
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