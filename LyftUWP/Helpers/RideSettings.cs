using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;

namespace LyftUWP.Helpers
{
    public class RideSettings
    {
        public static BasicGeoposition PICKUP_POINT
        {
            get { return new BasicGeoposition { Latitude = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["PICKUP_POINT_LATITUDE"].ToString()), Longitude = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["PICKUP_POINT_LONGITUDE"].ToString()) }; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["PICKUP_POINT_LATITUDE"] = value.Latitude;
                ApplicationData.Current.LocalSettings.Values["PICKUP_POINT_LONGITUDE"] = value.Longitude;
            }
        }

        public static string PICKUP_ADDRESS
        {
            get { return ApplicationData.Current.LocalSettings.Values["PICKUP_ADDRESS"].ToString(); }
            set { ApplicationData.Current.LocalSettings.Values["PICKUP_ADDRESS"] = value; }
        }

        public static BasicGeoposition DESTINATION_POINT
        {
            get { return new BasicGeoposition { Latitude = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["DESTINATIO_POINT_LATITUDE"].ToString()), Longitude = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["DESTINATIO_POINT_LONGITUDE"].ToString()) }; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["DESTINATIO_POINT_LATITUDE"] = value.Latitude;
                ApplicationData.Current.LocalSettings.Values["DESTINATIO_POINT_LONGITUDE"] = value.Longitude;
            }
        }

        public static string DESTINATION_ADDRESS
        {
            get { return ApplicationData.Current.LocalSettings.Values["DESTINATION_ADDRESS"].ToString(); }
            set { ApplicationData.Current.LocalSettings.Values["DESTINATION_ADDRESS"] = value; }
        }

        public static string RideType { get; set; }

        public static string RideTypeResponse { get; set; }
    }
}
