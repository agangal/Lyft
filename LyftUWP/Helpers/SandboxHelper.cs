using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace LyftUWP.Helpers
{
    public class SandboxHelper
    {
        public static async Task<bool> RideStatesStatus(long rideid, string status)
        {
            string api = "https://api.lyft.com/v1/sandbox/rides/" + rideid.ToString();
            string data = "{\"status\" :\"" + status + "\"}";
            HttpResponseMessage message = await URIHelper.PutRequest(api, data);
            if (message.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public static async Task<bool> PresetRideTypes(double lat, double lng)
        {
            string api = "https://api.lyft.com/v1/sandbox/ridetypes";
            string data = "{\"lat\" :" + lat.ToString() + ", \"lng\":" + lng.ToString() + ",\"ride_types\":[\"lyft\", \"lyft_line\"]}";
            HttpResponseMessage message = await URIHelper.PutRequest(api, data);
            if (message.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public static async Task<bool> SetRideTypeAvailability(double lat, double lng, string ride_types, bool available)
        {
            string api = "https://api.lyft.com/v1/sandbox/ridetypes/" + ride_types;
            string data = "{\"lat\" :" + lat.ToString() + ", \"lng\":" + lng.ToString() + ",\"driver_availability\":true}";           
            if (!available)
            {
                data = "{\"lat\" :" + lat.ToString() + ", \"lng\":" + lng.ToString() + ",\"driver_availability\":false}";
            }
            HttpResponseMessage message = await URIHelper.PutRequest(api, data);
            if (message.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public static async void ChangeRideStateStatusPending()
        {
            long rideid = Convert.ToInt64(Settings.CURRENT_RIDE_ID);
            await RideStatesStatus(rideid, "pending");
        }

        public static async void ChangeRideStateStatusAccepted()
        {
            long rideid = Convert.ToInt64(Settings.CURRENT_RIDE_ID);
            await RideStatesStatus(rideid, "accepted");
        }

        public static async void ChangeRideStateStatusArrived()
        {
            long rideid = Convert.ToInt64(Settings.CURRENT_RIDE_ID);
            await RideStatesStatus(rideid, "arrived");
        }

        public static async void ChangeRideStateStatusPickedUp()
        {
            long rideid = Convert.ToInt64(Settings.CURRENT_RIDE_ID);
            await RideStatesStatus(rideid, "pickedup");
        }

        public static async void ChangeRideStateStatusDroppedOff()
        {
            long rideid = Convert.ToInt64(Settings.CURRENT_RIDE_ID);
            await RideStatesStatus(rideid, "droppedoff");
        }

        public static async void ChangeRideStateStatusCanceled()
        {
            long rideid = Convert.ToInt64(Settings.CURRENT_RIDE_ID);
            await RideStatesStatus(rideid, "canceled");
        }

        public static async void LyftLineAvailablity(bool availability, double lat, double lng)
        {
            await SetRideTypeAvailability(lat, lng, "lyft_line", availability);
        }

        public static async void LyftAvailablity(bool availability, double lat, double lng)
        {
            await SetRideTypeAvailability(lat, lng, "lyft", availability);
        }
    }
}
