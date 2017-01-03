﻿using System;
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
    using Windows.Services.Maps;
    using Windows.UI.Xaml.Controls.Maps;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RidesPage : Page
    {
        private Geolocator _geolocator = null;
        public ObservableCollection<TypeOfRide> TypeOfRideCollection { get; set; }
        public ObservableCollection<AddressSearch> AddressOptions { get; set; }
        private double lat;
        private double lon;
        public RidesPage()
        {
            this.InitializeComponent();
            TypeOfRideCollection = new ObservableCollection<TypeOfRide>();
            AddressOptions = new ObservableCollection<AddressSearch>();
            lat = 0;
            lon = 0;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RidesMap.LoadingStatusChanged += RidesMap_LoadingStatusChanged;
            
            Geoposition pos = await GetUserLocation();
            UpdateRideTypes(pos);
        }

        private void UpdateRideTypes(Geoposition pos)
        {            
            if (pos == null)
            {
                // Couldn't find location. Let user enter the address or move the map around
            }
            else
            {
                DownloadAvailableRideTypes(pos);
            }
        }

        public async void DownloadAvailableRideTypes(Geoposition pos)
        {
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
            if (TypeOfRideCollection.Count > 0)
            {
                RideTypeListView.SelectedIndex = 1;
                EtaInMinutes.Text = (rridetype.ride_types[1].eta_seconds / 60).ToString() + " MIN";
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _geolocator.PositionChanged -= _geolocator_PositionChanged;
            RidesMap.LoadingStatusChanged -= RidesMap_LoadingStatusChanged;
        }

        private async Task<Geoposition> GetUserLocation()
        {
            
            var accessStatus = await Geolocator.RequestAccessAsync();
            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                _geolocator = new Geolocator { ReportInterval = 5000, DesiredAccuracyInMeters = 200 };
                _geolocator.PositionChanged += _geolocator_PositionChanged;
                Geoposition pos = await _geolocator.GetGeopositionAsync();
                lat = pos.Coordinate.Point.Position.Latitude;
                lon = pos.Coordinate.Point.Position.Longitude;
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
                lat = position.Coordinate.Point.Position.Latitude;
                lon = position.Coordinate.Point.Position.Longitude;
            }
        }

        private void ShowRideTypes_Click(object sender, RoutedEventArgs e)
        {
           if (RideTypeListViewGrid.Visibility == Visibility.Visible)
            {
                RideTypeListViewGrid.Visibility = Visibility.Collapsed;
            }
           else
            {
                RideTypeListViewGrid.Visibility = Visibility.Visible;
            }
        }

        private void PickupAddressTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideSetPickupView();
            ShowAddressSearchView();
        }

        private void RideTypeListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedRideGrid.Visibility = Visibility.Visible;
            ShowRideTypesButtonGrid.Visibility = Visibility.Collapsed;
            TypeOfRide ridetype = e.ClickedItem as TypeOfRide;
            EtaInMinutes.Text = (ridetype.eta_seconds / 60).ToString() + " MIN";
        }

        private void RidesMap_LoadingStatusChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {
            
            Windows.UI.Xaml.Controls.Maps.MapControl mpc = sender as Windows.UI.Xaml.Controls.Maps.MapControl;
            if (sender.LoadingStatus == Windows.UI.Xaml.Controls.Maps.MapLoadingStatus.Loaded)
            {
                // PickupAddressTextBlock.Text = "Updating Address...";
                CoordinatesTextBlock.Text = mpc.ActualCamera.Location.Position.Latitude.ToString() + ", " + mpc.ActualCamera.Location.Position.Longitude.ToString();// RidesMap.Center.Position.Latitude.ToString() + ", " + RidesMap.Center.Position.Longitude.ToString();//mpc.ActualCamera.Location.Position.Latitude.ToString() + ", " + mpc.ActualCamera.Location.Position.Longitude.ToString();
                BasicGeoposition location = new BasicGeoposition { Latitude = mpc.ActualCamera.Location.Position.Latitude, Longitude = mpc.ActualCamera.Location.Position.Longitude, Altitude = mpc.ActualCamera.Location.Position.Altitude };
                GeocodeLocationMarker(location);
            }            //GeocodeLocationMarker(location);         
        }

        private async void GeocodeLocationMarker(BasicGeoposition location)
        {
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(new Geopoint(location));
            if (result.Status == MapLocationFinderStatus.Success)
            {
                var loc = result.Locations[0];
                PickupAddressTextBlock.Text = result.Locations[0].Address.FormattedAddress;
                //RidesMap.MapElements.Clear();
               // Geopoint myPoint = new Geopoint(new BasicGeoposition() { Latitude = 51, Longitude = 0 });
                //create POI
                //MapIcon myPOI = new MapIcon { Location = new Geopoint(location), Title = "My position", ZIndex = 0 };
                //// add to map and center it
                //RidesMap.MapElements.Add(myPOI);               
            }
        }

        private void CloseAddressSearchGridButton_Click(object sender, RoutedEventArgs e)
        {
            HideAddressSearchView();
            ShowSetPickupView();
        }        

        private void SetPickupButton_Click(object sender, RoutedEventArgs e)
        {
            HideSetPickupView();
            ShowSetDestinationView();
        }

        private void SetDestinationGrid_AddPickup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideSetDestinationView();
            ShowSetPickupView();
        }

        private void SetDestinationGrid_AddDestination_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private async void SearchBarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text.Length > 3)
            {
                AddressOptions.Clear();
                BasicGeoposition pos = new BasicGeoposition { Latitude = lat, Longitude = lon };
                var results = await MapLocationFinder.FindLocationsAsync(tb.Text, new Geopoint(pos));
                if (results.Status == MapLocationFinderStatus.Success)
                {
                    foreach (var location in results.Locations)
                    {
                        AddressSearch adsearch = new AddressSearch
                        {
                            formatted_address = location.Address.FormattedAddress,
                            latitude = location.Point.Position.Latitude,
                            longitude = location.Point.Position.Longitude
                        };
                        AddressOptions.Add(adsearch);
                    }
                   
                }
            }
            else
            {
                AddressOptions.Clear();
            }
           
        }

        private void AddressSearchListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            RidesMap.LoadingStatusChanged -= RidesMap_LoadingStatusChanged;
            AddressSearch address = (AddressSearch)e.ClickedItem;
            BasicGeoposition pos = new BasicGeoposition { Latitude = address.latitude, Longitude = address.longitude };
            RidesMap.Center = new Geopoint(pos);
            HideAddressSearchView();
            ShowSetPickupView();
            SetMapZoomLevel(18);
           
        }

        private void SetDestinationButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetMapZoomLevel(int level)
        {
            RidesMap.ZoomLevel = level;
            //RidesMap.LoadingStatusChanged += RidesMap_LoadingStatusChanged;
        }

        private void HideAddressSearchView()
        {
            AddressSearchGrid.Visibility = Visibility.Collapsed;
            RidesMap.LoadingStatusChanged -= RidesMap_LoadingStatusChanged;
            MapElementsGrid.Visibility = Visibility.Visible;
        }

        private void ShowAddressSearchView()
        {
            RidesMap.LoadingStatusChanged -= RidesMap_LoadingStatusChanged;
            MapElementsGrid.Visibility = Visibility.Collapsed;
            AddressSearchGrid.Visibility = Visibility.Visible;
        }

        private void HideSetPickupView()
        {
            RideTypeListViewGrid.Visibility = Visibility.Collapsed;
            SetPickupGrid.Visibility = Visibility.Collapsed;
            SetPickupButton.Visibility = Visibility.Collapsed;
        }

        private void ShowSetPickupView()
        {
            RideTypeListViewGrid.Visibility = Visibility.Visible;
            SetPickupGrid.Visibility = Visibility.Visible;
            SetPickupButton.Visibility = Visibility.Visible;
        }

        private void HideSetDestinationView()
        {
            SetDestinationGrid.Visibility = Visibility.Collapsed;
            SetDestinationButton.Visibility = Visibility.Collapsed;
        }

        private void ShowSetDestinationView()
        {
            SetDestinationGrid.Visibility = Visibility.Visible;
            SetDestinationButton.Visibility = Visibility.Visible;
        }
       
    }
}
