using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityComprimation
{
    public class CCConfig
    {

        internal static bool PRINT_DEBUG = false;
        internal static bool totalDEBUG = false;
        public static bool TOTAL_PEFORMANCE = false;

        public static void debug(string msg) {
            if (PRINT_DEBUG)
                Debug.WriteLine(msg);
        }

    }
}
