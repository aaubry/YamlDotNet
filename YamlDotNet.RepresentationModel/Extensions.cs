using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YamlDotNet.RepresentationModel
{
    public static class Extensions
    {
        public static TextReader ToTextReader(this string str)
        {
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            return new StringReader(str);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string Capitalize(this string str)
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string Decapitalize(this string str)
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }

            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}
