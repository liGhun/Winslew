using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winslew.Formatter
{
    class prettyVersion
    {
        public static string getNiceVersionString(string fullString)
        {
            while(fullString.Length > 2 && fullString.EndsWith(".0")) {
                fullString = fullString.Substring(0,fullString.Length -2);
            }
            return fullString;
        }
    }
}
