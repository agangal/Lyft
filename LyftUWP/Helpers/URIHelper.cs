using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LyftUWP.Helpers
{
    using Windows.Web.Http;
    using Windows.Web.Http.Headers;

    public class URIHelper
    {
        public static string ACCESS_TOKEN_URI
        {
            get { return "https://api.lyft.com/oauth/token"; }
        }

        public static string AUTHORIZATION_CODE_URI
        {
            get { return "https://api.lyft.com/oauth/authorize"; }
        }

        public static string RIDE_TYPE_URI
        {
            get { return "https://api.lyft.com/v1/ridetypes"; }
        }

        public static string RIDE_TYPE_URI_LASTUSED
        {
            get { return ApplicationData.Current.LocalSettings.Values["RIDE_TYPE_URI_LASTUSED"].ToString(); }
            set { ApplicationData.Current.LocalSettings.Values["RIDE_TYPE_URI_LASTUSED"] = value; }
        }

        public static string ETA_URI
        {
            get { return "https://api.lyft.com/v1/eta"; }
        }

        public static string ETA_URI_LASTUSED
        {
            get { return ApplicationData.Current.LocalSettings.Values["ETA_URI_LASTUSED"].ToString(); }
            set { ApplicationData.Current.LocalSettings.Values["ETA_URI_LASTUSED"] = value; }
        }

        public static string RIDE_URI
        {
            get { return "https://api.lyft.com/v1/rides"; }
        }

        public static async Task<string> GetRequest(string api)
        {
            HttpClient httpclient = new HttpClient();
            try
            {
                httpclient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", Settings.ACCESS_TOKEN);
                var httpresponsemessage = await httpclient.GetAsync(new Uri(api));
                if (httpresponsemessage.IsSuccessStatusCode)
                {
                    return (await httpresponsemessage.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        public static async Task<HttpResponseMessage> PostRequest(string api, string data)
        {
            HttpClient httpclient = new HttpClient();
            try
            {
                httpclient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", Settings.ACCESS_TOKEN);
                httpclient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage httpresponsemessage = await httpclient.PostAsync(new Uri(api), new HttpStringContent(data));
                return httpresponsemessage;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
