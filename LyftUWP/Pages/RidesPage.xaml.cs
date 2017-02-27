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

    /*
     * 
     * 1. Move it around and hit Set pickup --> timer.Stop(); --> Set destination --> Click add pickup --> timer.Start()
     * 2. Search for an address --> Select address --> timer.Stop(), Wait a second --> timer.Start() -->Hit set pickup --> timer.stop
     *  
     */
namespace LyftUWP.Pages
{
    using LyftUWP.Helpers;
    using Model;
    using System.Collections.ObjectModel;
    using Windows.Services.Maps;
    using Windows.Storage.Streams;
    using Windows.System.Threading;
    using Windows.UI.Xaml.Controls.Maps;
    using Windows.UI.Xaml.Media.Imaging;
    using Windows.Web.Http;

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
        private DispatcherTimer timer;
        private double currentLat = 0;
        private double currentLng = 0;
        public RidesPage()
        {
            this.InitializeComponent();
            TypeOfRideCollection = new ObservableCollection<TypeOfRide>();
            AddressOptions = new ObservableCollection<AddressSearch>();
            lat = 0;
            lon = 0;
            timer = new DispatcherTimer();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            timer.Tick -= Timer_Tick;
            timer.Stop();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Geoposition pos = (Geoposition)e.Parameter;
            BasicGeoposition bpos = new BasicGeoposition { Latitude = pos.Coordinate.Point.Position.Latitude, Longitude = pos.Coordinate.Point.Position.Longitude, Altitude = pos.Coordinate.Point.Position.Altitude };
            RidesMap.Center = new Geopoint(bpos);
            SetMapZoomLevel(19);
            RidesMap.CenterChanged += RidesMap_CenterChanged;
            
            
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();
            
           // UpdateRideTypes(bpos);
        }

        private void Timer_Tick(object sender, object e)
        {
            if (RidesMap.Visibility == Visibility.Visible)
            {
                Geopoint center = RidesMap.Center;
                GeocodeLocationMarker(center);
            }
        }

        private void UpdateRideTypes(BasicGeoposition pos)
        {
            DownloadAvailableRideTypes(pos);           
        }

        public async void DownloadAvailableRideTypes(BasicGeoposition pos)
        {
            HttpResponseMessage ride_type_response = await URIHelper.GetRequest(URIHelper.RIDE_TYPE_URI + "?lat=" + pos.Latitude.ToString() + "&lng=" + pos.Longitude.ToString());
            if (ride_type_response.IsSuccessStatusCode)
            {
                HttpResponseMessage eta_response = await URIHelper.GetRequest(URIHelper.ETA_URI + "?lat=" + pos.Latitude.ToString() + "&lng=" + pos.Longitude.ToString());
                if (eta_response.IsSuccessStatusCode)
                {
                    string ride_type = await ride_type_response.Content.ReadAsStringAsync();
                    if (String.Compare(ride_type, RideSettings.RideTypeResponse, true) != 0)
                    {
                        RideSettings.RideTypeResponse = ride_type;
                        string eta = await eta_response.Content.ReadAsStringAsync();
                        //string ride_type = await URIHelper.GetRequest(URIHelper.RIDE_TYPE_URI + "?lat=47.61" + "&lng=-122.33");
                        //string eta = await URIHelper.GetRequest(URIHelper.ETA_URI + "?lat=47.61" + "&lng=-122.33");
                        RootObjectRideType rridetype = RideType.DataDeserializerRideType(ride_type);
                        RootObjectEtaEstimate retaestimate = RideType.DataDeserializerEtaEstimate(eta);
                        for (int i = 0; i < rridetype.ride_types.Count; i++)
                        {
                            for (int j = 0; j < retaestimate.eta_estimates.Count; j++)
                            {
                                if (retaestimate.eta_estimates[j].ride_type == rridetype.ride_types[i].ride_type)
                                {
                                    if (retaestimate.eta_estimates[j].eta_seconds != null)
                                    {
                                        rridetype.ride_types[i].eta_seconds = retaestimate.eta_estimates[j].eta_seconds;
                                        break;
                                    }
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
                            ChangeEtaMapIcon(rridetype.ride_types[1].eta_seconds);
                        }
                        LyftPresent.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    string error = await eta_response.Content.ReadAsStringAsync();
                    RideTypeErrorObject rerror = RideType.DataDeserializerError(error);
                    ChangeEtaMapIcon(-1);
                    LyftPresent.Visibility = Visibility.Visible;
                }
            }
            else
            {
                string error = await ride_type_response.Content.ReadAsStringAsync();
                RideTypeErrorObject rerror = RideType.DataDeserializerError(error);
                //if (rerror.)
                ChangeEtaMapIcon(-1);
                LyftPresent.Visibility = Visibility.Visible;
                //LyftNotPresent.Visibility = Visibility.Visible;
                //EtaInMinutesLocationMarker.Text = "Lyft is not available here";
                //LocationMarkerEllipseInner.Width = EtaInMinutesLocationMarker.Width + 5;
                //LocationMarkerEllipseInner.Height = EtaInMinutesLocationMarker.Height + 5;
                //LocationMarkerEllipseOuter.Width = LocationMarkerEllipseInner.Width + 3;
                //LocationMarkerEllipseOuter.Height = LocationMarkerEllipseInner.Height + 3;
            }
        }

        private void ChangeEtaMapIcon(int seconds)
        {
            int minutes = (int)seconds / 60;
            switch (minutes)
            {
                case 1:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min1_2x.png"));
                    break;
                case 2:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min2_2x.png"));
                    break;
                case 3:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min3_2x.png"));
                    break;
                case 4:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min4_2x.png"));
                    break;
                case 5:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min5_2x.png"));
                    break;
                case 6:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min6_2x.png"));
                    break;
                case 7:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min7_2x.png"));
                    break;
                case 8:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min8_2x.png"));
                    break;
                case 9:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min9@2x.png"));
                    break;
                case 10:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min10.png"));
                    break;
                case 11:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min11.png"));
                    break;
                case 12:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min12.png"));
                    break;
                case 13:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min13.png"));
                    break;
                case 14:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min14.png"));
                    break;
                case 15:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min15.png"));
                    break;
                case 16:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min16.png"));
                    break;
                case 17:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min17.png"));
                    break;
                case 18:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min18.png"));
                    break;
                case 19:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min19.png"));
                    break;
                case 20:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min20.png"));
                    break;
                case 21:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min21.png"));
                    break;
                case 22:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min22.png"));
                    break;
                case 23:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min23.png"));
                    break;
                case 24:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min24.png"));
                    break;
                case 25:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min25.png"));
                    break;
                case 26:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min26.png"));
                    break;
                case 27:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min27.png"));
                    break;
                case 28:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min28.png"));
                    break;
                case 29:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min29.png"));
                    break;
                case 30:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/min30.png"));
                    break;
                case -1:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/lyftnotavailable_2x.png"));
                    break;
                default:
                    EtaMapIcon.Source = new BitmapImage(new Uri("ms-appx:///Images/MapMarker/mingreater30.png"));
                    break;               
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _geolocator.PositionChanged -= _geolocator_PositionChanged;
        }

        

        private async void _geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //UpdateUserLocation(args.Position);
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
            ChangeEtaMapIcon(ridetype.eta_seconds);
        }

        private async void GeocodeLocationMarker(Geopoint location)
        {
            if (currentLat != location.Position.Latitude || currentLng != location.Position.Longitude)
            {
                MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(location);
                if (result.Status == MapLocationFinderStatus.Success)
                {
                    if (result.Locations != null)
                    {
                        var locList = result.Locations.ToList();
                        if (locList.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Formatted Address : " + locList[0].Address.FormattedAddress + ", Display Name : " + locList[0].DisplayName);

                            PickupAddressTextBlock.Text = locList[0].Address.FormattedAddress;
                            BasicGeoposition pos = new BasicGeoposition { Latitude = locList[0].Point.Position.Latitude, Longitude = locList[0].Point.Position.Longitude, Altitude = locList[0].Point.Position.Altitude };
                            UpdateRideTypes(pos);
                        }
                        //RidesMap.MapElements.Clear();
                        //Geopoint myPoint = new Geopoint(new BasicGeoposition() { Latitude = 51, Longitude = 0 });
                        ////create POI
                        //MapIcon myPOI = new MapIcon { Location = location, Title = "My position", ZIndex = 0 };
                        ////// add to map and center it
                        //RidesMap.MapElements.Add(myPOI);       
                    }
                }
            }
        }

        private void CloseAddressSearchGridButton_Click(object sender, RoutedEventArgs e)
        {
            HideAddressSearchView();
            ShowSetPickupView();
        }        

        private void SetPickupButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();
            AddMapIcon(RideSettings.PICKUP_POINT, RideSettings.PICKUP_ADDRESS, "ms-appx:///Images/PickupIcon.png");
            HideSetPickupView();
            ShowSetDestinationView();
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void SetDestinationGrid_AddPickup_Tapped(object sender, TappedRoutedEventArgs e)
        {
            timer.Tick += Timer_Tick;
            timer.Start();
            HideSetDestinationView();
            ShowSetPickupView();
        }

        private void SetDestinationGrid_AddDestination_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideSetDestinationView();
            ShowAddressSearchView(true);
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

        private async void AddressSearchListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();
            AddressSearch address = (AddressSearch)e.ClickedItem;
            BasicGeoposition pos = new BasicGeoposition { Latitude = address.latitude, Longitude = address.longitude };
            RideSettings.PICKUP_ADDRESS = address.formatted_address;
            RideSettings.PICKUP_POINT = pos;
            PickupAddressTextBlock.Text = address.formatted_address;
            SetDestinationGrid_AddPickup.Text = address.formatted_address;
            RidesMap.Center = new Geopoint(pos);
            
            RidesMap.CenterChanged -= RidesMap_CenterChanged;
            SetMapZoomLevel(19);
            HideAddressSearchView();
            ShowSetPickupView();
            await Task.Delay(1500);
            //timer.Tick += Timer_Tick;
            //timer.Start();
            RidesMap.CenterChanged += RidesMap_CenterChanged;
        }

        private void SetDestinationButton_Click(object sender, RoutedEventArgs e)
        {
            //HideSetDestinationView()
        }

        private void SetMapZoomLevel(int level)
        {
            RidesMap.ZoomLevel = level;
            //RidesMap.LoadingStatusChanged += RidesMap_LoadingStatusChanged;
        }

        private void HideAddressSearchView()
        {
            PickUpAddressSearchGrid.Visibility = Visibility.Collapsed;
            DestinationAddressSearchGrid.Visibility = Visibility.Collapsed;
            DestinationSearchBarTextBox.Text = "";
            SearchBarTextBox.Text = "";        
            MapElementsGrid.Visibility = Visibility.Visible;
        }

        private void ShowAddressSearchView(bool setDestination = false)
        {            
            MapElementsGrid.Visibility = Visibility.Collapsed;
            if (setDestination)
            {
                DestinationAddressSearchGrid.Visibility = Visibility.Visible;
            }
            else
            {
                PickUpAddressSearchGrid.Visibility = Visibility.Visible;
            }
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

        private void RidesMap_CenterChanged(MapControl sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("Center Changed");
            if (!timer.IsEnabled)
            {
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private async void DestinationSearchBarTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
        
        private void DestinationAddressSearchListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            AddressSearch address = (AddressSearch)e.ClickedItem;
            BasicGeoposition pos = new BasicGeoposition { Latitude = address.latitude, Longitude = address.longitude };
            //RidesMap.Center = new Geopoint(pos);
            //RidesMap.CenterChanged -= RidesMap_CenterChanged;
            SetDestinationGrid_AddDestination.Text = address.formatted_address;
            RideSettings.DESTINATION_ADDRESS = address.formatted_address;
            RideSettings.DESTINATION_POINT = new BasicGeoposition { Latitude = address.latitude, Longitude = address.longitude };
            AddMapIcon(RideSettings.DESTINATION_POINT, RideSettings.DESTINATION_ADDRESS, "ms-appx:///Images/DestinationIcon.png");
            SetMapZoomLevel(19);
            HideAddressSearchView();
            ShowSetDestinationView();
            //await Task.Delay(1000);
            //RidesMap.CenterChanged += RidesMap_CenterChanged;
        }

        private void AddMapIcon(BasicGeoposition position, string label, string pathToImage)
        {
            //RidesMap.MapElements.Clear();
            Geopoint myPoint = new Geopoint(position);
            ////create POI
            MapIcon myPOI = new MapIcon { Location = myPoint, Title = label, ZIndex = 0, NormalizedAnchorPoint = new Point(0.5,1.0) };
            myPOI.Image = RandomAccessStreamReference.CreateFromUri(new Uri(pathToImage));
            ////// add to map and center it
            RidesMap.MapElements.Add(myPOI);
            var elem = RidesMap.MapElements.ToList();
        }

        private void ChangeUIForHttpResponse(HttpResponseMessage message)
        {

        }
    }
}
