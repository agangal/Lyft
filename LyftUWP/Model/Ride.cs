using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyftUWP.Model
{
    using Newtonsoft.Json;

    public class Ride
    {
        public static RideRequest DeserializeRideRequest(string res)
        {
            RideRequest rreq = JsonConvert.DeserializeObject<RideRequest>(res);
            return rreq;
        }

        public static string SerializeRideRequest(RideRequest rreq)
        {
            string res = JsonConvert.SerializeObject(rreq);
            return res;
        }

        public static string SerializePrimeTimeConfirm(RideRequestPrimeTime rreqp)
        {
            string res = JsonConvert.SerializeObject(rreqp);
            return res;
        }
        public static RideResponse DeserializeRideResponse(string res)
        {
            RideResponse rres = JsonConvert.DeserializeObject<RideResponse>(res);
            return rres;
        }
    }

    public class Origin
    {
        public double lat { get; set; }
        public double lng { get; set; }
        public string address { get; set; }
    }

    public class Destination
    {
        public double lat { get; set; }
        public double lng { get; set; }
        public string address { get; set; }
    }

    public class RideRequest
    {
        public string ride_type { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
    }

    public class RideRequestPrimeTime
    {
        public string ride_id { get; set; }
        public string ride_type { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
        public string primetime_confirmation_token { get; set; }
    }

    public class Passenger
    {
        public string first_name { get; set; }
        public string image_url { get; set; }
        public double rating { get; set; }
        public string user_id { get; set; }
    }

    public class RideResponse
    {
        public int ride_id { get; set; }
        public string status { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
        public Passenger passenger { get; set; }
    }
}
