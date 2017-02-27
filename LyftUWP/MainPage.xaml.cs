using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LyftUWP
{
    using Helpers;
    using Pages;
    using System.Threading.Tasks;
    using Windows.Devices.Geolocation;
    using Windows.Web.Http;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (!String.IsNullOrEmpty(Settings.AUTHORIZATION_CODE))
            {
                HttpResponseMessage message = await AuthHelper.CheckIfAccessTokenIsValid();
                if (message != null && message.IsSuccessStatusCode)
                {
                    GoToRidesPage();
                }
                else
                {
                    message = await AuthHelper.RefreshAccessToken();
                    if (message != null && message.IsSuccessStatusCode)
                    {
                        GoToRidesPage();
                    }
                }
                
            }
        }

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            HttpResponseMessage message = new HttpResponseMessage();
       
            if (!String.IsNullOrEmpty(Settings.AUTHORIZATION_CODE))
            {
                message = await AuthHelper.CheckIfAccessTokenIsValid();
                if (message != null && !message.IsSuccessStatusCode)
                {
                    if (!String.IsNullOrEmpty(Settings.REFRESH_TOKEN))
                    {
                        message = await AuthHelper.RefreshAccessToken();
                        if (message != null && message.IsSuccessStatusCode)
                        {
                            GoToRidesPage();
                        }
                        else
                        {
                            if (await Helpers.AuthHelper.UserSignIn())
                            {
                                GoToRidesPage();
                            }
                        }
                    }
                    else
                    {
                        if (await AuthHelper.UserSignIn())
                        {
                            GoToRidesPage();
                        }
                    }               
                }
                else
                {
                    if (await Helpers.AuthHelper.UserSignIn())
                    {
                        GoToRidesPage();
                    }
                }
            }
            else
            {
                if (await Helpers.AuthHelper.UserSignIn())
                {
                    GoToRidesPage();
                }
            }            
        }

        private async void GoToRidesPage()
        {
            Geoposition pos = await GetUserLocation();
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(RidesPage), pos);
        }

        private async Task<Geoposition> GetUserLocation()
        {
            Geolocator _geolocator = null;
            var accessStatus = await Geolocator.RequestAccessAsync();
            double lat = 0;
            double lng = 0;
            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                _geolocator = new Geolocator { ReportInterval = 5000, DesiredAccuracyInMeters = 200 };
               // _geolocator.PositionChanged += _geolocator_PositionChanged;
                Geoposition pos = await _geolocator.GetGeopositionAsync();
                lat = pos.Coordinate.Point.Position.Latitude;
                lng = pos.Coordinate.Point.Position.Longitude;
                return pos;
                //_geolocator.StatusChanged += _geolocator_StatusChanged;               
            }
            return null;
        }
    }
}
