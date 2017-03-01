﻿using System;
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
            string authurl = URIHelper.AUTHORIZATION_CODE_URI + "?client_id=" + Settings.CLIENT_ID + "&response_type=code&scope=offline public rides.read profile rides.request&state=" + Settings.STATE;
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

        public static async Task<bool> GetSandboxAccessToken()
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                string content = "client_id="+ Settings.CLIENT_ID + "&client_secret=SANDBOX-" + Settings.CLIENT_SECRET + "&redirect_uri=https%3A%2F%2Fashishgangal.com%2F&grant_type=authorization_code&state="+ Settings.STATE + "&code=" + Settings.AUTHORIZATION_CODE;
                HttpStringContent stringcont = new HttpStringContent(content);                
                var bytearray = Encoding.ASCII.GetBytes(Settings.CLIENT_ID + ":SANDBOX-"+ Settings.CLIENT_SECRET);
                httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytearray));
                stringcont.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                var httpresponseMessage = await httpClient.PostAsync(new Uri(URIHelper.ACCESS_TOKEN_URI), stringcont);
                if (httpresponseMessage.IsSuccessStatusCode)
                {
                    string resp = await httpresponseMessage.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<AccessTokenObject>(resp);
                    
                         
                    StoreSandboxBearerToken(obj.access_token);
                    //StoreAccessToken(obj.access_token);
                    StoreRefreshToken(obj.refresh_token);
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

        public static async Task<bool> GetAccessToken()
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                string content = "client_id=" + Settings.CLIENT_ID + "&client_secret=" + Settings.CLIENT_SECRET + "&redirect_uri=https%3A%2F%2Fashishgangal.com%2F&grant_type=authorization_code&state=" + Settings.STATE + "&code=" + Settings.AUTHORIZATION_CODE;
                HttpStringContent stringcont = new HttpStringContent(content);
                var bytearray = Encoding.ASCII.GetBytes(Settings.CLIENT_ID + ":" + Settings.CLIENT_SECRET);
                httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytearray));
                stringcont.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                var httpresponseMessage = await httpClient.PostAsync(new Uri(URIHelper.ACCESS_TOKEN_URI), stringcont);
                if (httpresponseMessage.IsSuccessStatusCode)
                {
                    string resp = await httpresponseMessage.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<AccessTokenObject>(resp);
                    StoreAccessToken(obj.access_token);
                    StoreRefreshToken(obj.refresh_token);
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

        public static async Task<HttpResponseMessage> CheckIfAccessTokenIsValid()
        {
            HttpClient httpclient = new HttpClient();
            try
            {
                httpclient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Bearer", Settings.ACCESS_TOKEN);
                var httpresponsemessage = await httpclient.GetAsync(new Uri("https://api.lyft.com/v1/ridetypes?lat=37.7833&lng=-122.4167"));
                return (httpresponsemessage);              
            }
            catch (Exception ex)
            {
                return null;
            }            
        }

        private static void StoreAccessToken(string accesstoken)
        {
            Settings.ACCESS_TOKEN = accesstoken;
        }

        public static void StoreSandboxBearerToken(string token)
        {
            string conststr = "SANDBOX-";
            string bearertoken = token.Substring(conststr.Length, token.Length - conststr.Length);
            Settings.ACCESS_TOKEN = bearertoken;
        }
        private static void StoreRefreshToken(string refreshtoken)
        {
            Settings.REFRESH_TOKEN = refreshtoken;
        }
        public static async Task<HttpResponseMessage> RefreshAccessToken()
        {
            HttpClient httpClient = new HttpClient();
            try
            {                
                string content = "client_id=" + Settings.CLIENT_ID + "&client_secret=" + Settings.CLIENT_SECRET + "&redirect_uri=https%3A%2F%2Fashishgangal.com%2F&grant_type=refresh_token&state=" + Settings.STATE + "&refresh_token=" + Settings.REFRESH_TOKEN;
                HttpStringContent stringcont = new HttpStringContent(content);
                var bytearray = Encoding.ASCII.GetBytes(Settings.CLIENT_ID + ":" + Settings.CLIENT_SECRET);
                httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytearray));
                stringcont.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                var httpresponseMessage = await httpClient.PostAsync(new Uri(URIHelper.ACCESS_TOKEN_URI), stringcont);
                if (httpresponseMessage.IsSuccessStatusCode)
                {
                    string resp = await httpresponseMessage.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<AccessTokenObject>(resp);
                    StoreAccessToken(obj.access_token);                
                }
                return httpresponseMessage;
            }
            catch (Exception ex)
            {
                
            }
            return null;
        }

        public static async Task<HttpResponseMessage> RefreshSandboxAccessToken()
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                string content = "client_id=" + Settings.CLIENT_ID + "&client_secret=SANDBOX-" + Settings.CLIENT_SECRET + "&redirect_uri=https%3A%2F%2Fashishgangal.com%2F&grant_type=refresh_token&state=" + Settings.STATE + "&refresh_token=" + Settings.REFRESH_TOKEN;
                HttpStringContent stringcont = new HttpStringContent(content);
                var bytearray = Encoding.ASCII.GetBytes(Settings.CLIENT_ID + ":SANDBOX-" + Settings.CLIENT_SECRET);
                httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytearray));
                stringcont.Headers.ContentType = new HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                var httpresponseMessage = await httpClient.PostAsync(new Uri(URIHelper.ACCESS_TOKEN_URI), stringcont);
                if (httpresponseMessage.IsSuccessStatusCode)
                {
                    string resp = await httpresponseMessage.Content.ReadAsStringAsync();
                    var obj = JsonConvert.DeserializeObject<AccessTokenObject>(resp);
                    //StoreAccessToken(obj.access_token);
                    //StoreRefreshToken(obj.refresh_token);                      
                    StoreSandboxBearerToken(obj.access_token);
                }
                return httpresponseMessage;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static async Task<bool> UserSignIn()
        {
            bool code_result = await GetAuthorizationCode();
            if (code_result == true)
            {
                bool token_result = false;
                if (Settings.USE_SANDBOX)
                {
                    token_result = await GetSandboxAccessToken();                    
                }
                else
                {
                    token_result = await GetAccessToken();                   
                }
                return token_result;
            }
            return false;
        }

        public class AccessTokenObject
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string scope { get; set; }            
        }
    }
}
