using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ISS.Common
{
    public static class Extensions
    {
         
    }
    public static class StringExtensions
    {
        public static string To_IncreaseNumber(this string value, int incremental)
        {
            var hasnumber = value.FindFirstNumber();
            if (hasnumber.HasValue)
            {
                return value.Replace(hasnumber.Value.ToString(), (hasnumber.Value + incremental).ToString());
            }
            return value;
        }
        public static int? FindFirstNumber(this string value)
        {
            var match = Regex.Match(value, @"\d+");
            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            return null;
        }
    }
}
