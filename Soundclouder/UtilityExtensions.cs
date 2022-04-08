using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundclouder;
public static class UtilityExtensions
{
    public static string MakeStringURLFriendly(this string str)
    {
        //TODO: Find a better and universal way to do this.
        var newStr = str.Replace(" ", "%20").Replace("ø", "%C3%B8").Replace("å", "%C3%A5");
        return newStr;
    }
}
