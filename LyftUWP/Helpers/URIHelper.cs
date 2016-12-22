using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
