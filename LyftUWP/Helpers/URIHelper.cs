﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LyftUWP.Helpers
{
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
    }
}
