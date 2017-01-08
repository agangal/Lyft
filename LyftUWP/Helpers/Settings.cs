using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyftUWP.Helpers
{
    public class Settings
    {
        public static string CLIENT_ID { get; }
        public static string STATE { get; }
        public static string AUTHORIZATION_CODE { get; set; }
        public static string CLIENT_SECRET { get; set; }
        public static string ACCESS_TOKEN { get; set; }
    }
}
