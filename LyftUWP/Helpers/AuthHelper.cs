using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace LyftUWP.Helpers
{
    public class AuthHelper
    {
        public static async Task<bool> GetAuthorizationCode()
        {
            string authurl = "https://api.lyft.com/oauth/authorize?client_id=" + Settings.CLIENT_ID + "&response_type=code&scope=public&state=" + Settings.STATE;
            Uri StartUri = new Uri(authurl);
            Uri EndUri = new Uri("https://ashishgangal.com/");
            WebAuthenticationResult WebAuthResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, EndUri);
            if (WebAuthResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                string responsedata = WebAuthResult.ResponseData.ToString();
                string substring = responsedata.Substring(responsedata.IndexOf("code"));
                string[] keyValuePairs = substring.Split('&');
                string[] splits = keyValuePairs[0].Split('=');
                string code = splits[1];
                System.Diagnostics.Debug.WriteLine(code);
            }
            return true;
        }
    }
}
