using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LyftUWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RidesPage : Page
    {
        private Geolocator _geolocator = null;
        public RidesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GetUserLocation();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _geolocator.PositionChanged -= _geolocator_PositionChanged;
        }

        private async void GetUserLocation()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                _geolocator = new Geolocator { ReportInterval = 5000, DesiredAccuracyInMeters = 200 };
                _geolocator.PositionChanged += _geolocator_PositionChanged;
                //_geolocator.StatusChanged += _geolocator_StatusChanged;               
            }
        }

        private async void _geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                UpdateUserLocation(args.Position);
            });
        }

        private void UpdateUserLocation(Geoposition position)
        {
            if (position != null)
            {
                System.Diagnostics.Debug.WriteLine(position.Coordinate.Point.Position.Latitude.ToString() + ", " + position.Coordinate.Point.Position.Longitude.ToString());
            }
        }
    }
}
