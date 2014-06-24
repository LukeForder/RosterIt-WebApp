using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RosterIt.Web.Models
{
    public class PseudoNumericComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {   
            int xInt, yInt;

            bool xIsInt = int.TryParse(x, out xInt), 
                 yIsInt = int.TryParse(y, out yInt);

            if (xIsInt && yIsInt)
                return xInt == yInt ? 0 : xInt < yInt ? -1 : 1;
            else if (xIsInt && !yIsInt)
                return -1;
            else if (!xIsInt && yIsInt)
                return 1;
            else
                return string.Compare(x, y, true);
        }
    }
}