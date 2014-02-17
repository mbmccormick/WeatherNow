using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System.Device.Location;
using Weatherman.API;
using Microsoft.Phone.Shell;

namespace Weatherman.Background
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        static ScheduledAgent()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        protected override void OnInvoke(ScheduledTask task)
        {
            GeoCoordinateWatcher locationService = new GeoCoordinateWatcher();
            locationService.Start();

            GeoPosition<GeoCoordinate> position = locationService.Position;

            double latitude = position.Location.Latitude;
            double longitude = position.Location.Longitude;

            ForecastIORequest request = new ForecastIORequest("29b5b5502df6a3e0c0446fb72ded0a97", latitude, longitude, Unit.auto);
            request.Get((result) =>
            {
                bool precipitationFound = false;
                for (int i = 0; i < 19; i++)
                {
                    if (result.minutely.data[i].precipType == "rain")
                    {
                        precipitationFound = true;
                    }
                }

                if (precipitationFound == false &&
                    result.minutely.data[20].precipType == "rain")
                {
                    // display toast
                    ShellToast notification = new ShellToast();
                    notification.Title = "Weatherman";
                    notification.Content = "Rain starting in 20 minutes.";
                }
            });

            NotifyComplete();
        }
    }
}