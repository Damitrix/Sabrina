// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpers.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Helpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities
{
    using Sabrina.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;

    /// <summary>
    /// An helper class
    /// </summary>
    public static class Helpers
    {
        private static char[] localInvalidChars = new char[] { '\"' };

        public static bool ContainsKey(this List<KeyValuePair<UserSettingExtension.SettingID, UserSetting>> valueList, UserSettingExtension.SettingID setting)
        {
            return valueList.ContainsKey(setting);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string GetSafeFilename(string filename)
        {
            var text = string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
            return string.Join("_", text.Split(localInvalidChars));
        }

        public static string ToFormattedText(this Enum value)
        {
            var stringVal = value.ToString();
            var bld = new System.Text.StringBuilder();

            for (var i = 0; i < stringVal.Length; i++)
            {
                if (char.IsUpper(stringVal[i]))
                {
                    bld.Append(" ");
                }

                bld.Append(stringVal[i]);
            }

            return bld.ToString();
        }

        public static class RandomGenerator
        {
            private static readonly RNGCryptoServiceProvider Rand = new RNGCryptoServiceProvider();

            /// <summary>
            /// Generates a random Integer in a specified Range
            /// </summary>
            /// <param name="min">Inclusive minimal number</param>
            /// <param name="max">Exclusive maximal number</param>
            /// <returns></returns>
            public static int RandomInt(int min, int max)
            {
                uint scale = uint.MaxValue;
                while (scale == uint.MaxValue)
                {
                    // Get four random bytes.
                    byte[] four_bytes = new byte[4];
                    Rand.GetBytes(four_bytes);

                    // Convert that into an uint.
                    scale = BitConverter.ToUInt32(four_bytes, 0);
                }

                // Add min to the scaled difference between max and min.
                return (int)(min + (max - min) *
                             (scale / (double)uint.MaxValue));
            }
        }

        public static class RegexHelper
        {
            public static Regex AddRegex = new Regex("[Aa][Dd]?[Dd]?");
            public static Regex CancelRegex = new Regex("\\b[Cc][Aa][Nn][Cc][Ee][Ll]\\b");
            public static Regex ConfirmRegex = new Regex("\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b");
            public static Regex DoneRegex = new Regex("\\b[Dd][Oo][Nn][Ee]\\b");
            public static Regex ExitRegex = new Regex("\\b[Ee][Xx][Ii][Tt]\\b|" + CancelRegex.ToString() + "|" + DoneRegex.ToString());
            public static Regex NoRegex = new Regex("[Nn][Oo]?");
            public static Regex RemoveRegex = new Regex("[Rr][Ee][Mm][Oo][Vv][Ee]");
            public static Regex YesRegex = new Regex("[Yy][Ee]?[Ss]?");
        }
    }
}