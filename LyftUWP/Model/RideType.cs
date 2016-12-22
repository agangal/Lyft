using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyftUWP.Model
{
    using Newtonsoft.Json;

    public class RideType
    {
        public static RootObjectRideType DataDeserializerRideType(string data)
        {
            RootObjectRideType rridetype = JsonConvert.DeserializeObject<RootObjectRideType>(data);
            return rridetype;
        }

        public static RootObjectEtaEstimate DataDeserializerEtaEstimate(string data)
        {
            RootObjectEtaEstimate retaestimate = JsonConvert.DeserializeObject<RootObjectEtaEstimate>(data);
            return retaestimate;
        }
    }
    public class PricingDetails
    {
        public int base_charge { get; set; }
        public int cost_per_mile { get; set; }
        public int cost_per_minute { get; set; }
        public int cost_minimum { get; set; }
        public int trust_and_service { get; set; }
        public string currency { get; set; }
        public int cancel_penalty_amount { get; set; }
    }

    public class TypeOfRide
    {
        public string ride_type { get; set; }
        public string display_name { get; set; }
        public string image_url { get; set; }
        public int eta_seconds { get; set; }
        public PricingDetails pricing_details { get; set; }
        public int seats { get; set; }
    }

    public class RootObjectRideType
    {
        public List<TypeOfRide> ride_types { get; set; }
    }

    public class EtaEstimate
    {
        public string display_name { get; set; }
        public string ride_type { get; set; }
        public int eta_seconds { get; set; }
        public bool is_valid_estimate { get; set; }
    }

    public class RootObjectEtaEstimate
    {
        public List<EtaEstimate> eta_estimates { get; set; }
    }


}
