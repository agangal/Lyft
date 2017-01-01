using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace LyftUWP.Helpers
{
    using Windows.Web.Http;
    using Windows.Web.Http.Headers;
    using Newtonsoft.Json;
    public class AuthHelper
    {
        public static async Task<bool> GetAuthorizationCode()
        {
            string authurl = URIHelper.AUTHORIZATION_CODE_URI + "?client_id=" + Settings.CLIENT_ID + "&response_type=code&scope=public&state=" + Settings.STATE;
            Uri StartUri = new Uri(authurl);
            Uri EndUri = new Uri("https://ashishgangal.com/");
            WebAuthenticationResult WebAuthResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, EndUri);
            if (WebAuthResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                string responsedata = WebAuthResult.ResponseData.ToString();
                string substring = responsedata.Substring(responsedata.IndexOf("code"));
                string[] keyValuePairs = substring.Split('&');
                string[] splits = keyValuePairs[0].Split('=');
                Settings.AUTHORIZATION_CODE = splits[1];
                System.Diagnostics.Debug.WriteLine(Settings.AUTHORIZATION_CODE);
            }
            if (WebAuthResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> GetAccessToken()
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                string content = "client_id="+ Settings.CLIENT_ID + "&client_secret=" + Settings.CLIENT_SECRET + "&redirect_uri=https%3A%2F%2Fashishgangal.com%2F&grant_type=authorization_code&state="+ Settings.STATE + "&code=" + Settings.AUTHORIZATION_CODE;
                HttpStringContent stringcont = new HttpStringContent(content);                
                var bytearray = Encoding.ASCII.GetBytes(Settings.CLIENT_ID + ":"+ Settings.CLIENT_SECRET);
                httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytearray));
                stringcont.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                var httpresponseMessage = await httpClient.PostAsync(new Uri(URIHelper.ACCESS_TOKEN_URI), stringcont);
                if (httpresponseMessage.IsSuccessStatusCode)
                {
                    string resp = await httpresponseMessage.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<AccessTokenObject>(resp);
                    string conststr = "SANDBOX-";                  
                    Settings.ACCESS_TOKEN = obj.access_token.Substring(conststr.Length, obj.access_token.Length - conststr.Length);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> CheckIfAccessTokenIsValid()
        {
            HttpClient httpclient = new HttpClient();
            try
            {
                httpclient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", Settings.ACCESS_TOKEN);
                var httpresponsemessage = await httpclient.GetAsync(new Uri("https://api.lyft.com/v1/ridetypes?lat=37.7833&lng=-122.4167"));
                return (httpresponsemessage.IsSuccessStatusCode);              
            }
            catch (Exception ex)
            {
                return false;
            }            
        }
        //public static async Task<bool> RefreshAccessToken()
        //{
        //    HttpClient httpClient = new HttpClient();
        //    try
        //    {
        //        string content = "client_id=" + Settings.CLIENT_ID + "&client_secret=" + Settings.CLIENT_SECRET + "&redirect_uri=https%3A%2F%2Fashishgangal.com%2F&grant_type=authorization_code&state=" + Settings.STATE + "&code=" + Settings.AUTHORIZATION_CODE;
        //        HttpStringContent stringcont = new HttpStringContent(content);
        //        var bytearray = Encoding.ASCII.GetBytes(Settings.CLIENT_ID + ":" + Settings.CLIENT_SECRET);
        //        httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytearray));
        //        stringcont.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
        //        var httpresponseMessage = await httpClient.PostAsync(new Uri(Settings.ACCESS_TOKEN_URI), stringcont);
        //        string resp = await httpresponseMessage.Content.ReadAsStringAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        public static async Task<bool> UserSignIn()
        {
            bool code_result = await GetAuthorizationCode();
            if (code_result == true)
            {
                bool token_result = await GetAccessToken();
                return token_result;
            }
            return false;
        }

        public class AccessTokenObject
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
        }
    }
}
