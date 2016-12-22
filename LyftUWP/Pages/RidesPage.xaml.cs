using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    using LyftUWP.Helpers;
    using Model;
    using System.Collections.ObjectModel;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RidesPage : Page
    {
        private Geolocator _geolocator = null;
        public ObservableCollection<TypeOfRide> TypeOfRideCollection { get; set; }
        public RidesPage()
        {
            this.InitializeComponent();
            TypeOfRideCollection = new ObservableCollection<TypeOfRide>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateRideTypes();
        }

        private async void UpdateRideTypes()
        {
            Geoposition pos = await GetUserLocation();
            if (pos == null)
            {

            }
            else
            {
                // Get type of rides available
                //string ride_type = await URIHelper.GetRequest(URIHelper.RIDE_TYPE_URI + "?lat=" + pos.Coordinate.Point.Position.Latitude.ToString() + "&lng=" + pos.Coordinate.Point.Position.Longitude.ToString());
                //string eta = await URIHelper.GetRequest(URIHelper.ETA_URI + "?lat=" + pos.Coordinate.Point.Position.ToString() + "&lng=" + pos.Coordinate.Point.Position.Longitude.ToString());
                string ride_type = await URIHelper.GetRequest(URIHelper.RIDE_TYPE_URI + "?lat=47.61" + "&lng=-122.33");
                string eta = await URIHelper.GetRequest(URIHelper.ETA_URI + "?lat=47.61" + "&lng=-122.33");
                RootObjectRideType rridetype = RideType.DataDeserializerRideType(ride_type);
                RootObjectEtaEstimate retaestimate = RideType.DataDeserializerEtaEstimate(eta);
                for (int i = 0; i < rridetype.ride_types.Count; i++)
                {
                    for (int j = 0; j < retaestimate.eta_estimates.Count; j++)
                    {
                        if (retaestimate.eta_estimates[j].ride_type == rridetype.ride_types[i].ride_type)
                        {
                            rridetype.ride_types[i].eta_seconds = retaestimate.eta_estimates[j].eta_seconds;
                            break;
                        }
                    }
                }
                TypeOfRideCollection.Clear();
                for (int i = 0; i < rridetype.ride_types.Count; i++)
                {
                    TypeOfRideCollection.Add(rridetype.ride_types[i]);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _geolocator.PositionChanged -= _geolocator_PositionChanged;
        }

        private async Task<Geoposition> GetUserLocation()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                _geolocator = new Geolocator { ReportInterval = 5000, DesiredAccuracyInMeters = 200 };
                _geolocator.PositionChanged += _geolocator_PositionChanged;
                Geoposition pos = await _geolocator.GetGeopositionAsync();
                return pos;
                //_geolocator.StatusChanged += _geolocator_StatusChanged;               
            }
            return null;
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
