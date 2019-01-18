// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpers.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Helpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Sabrina.Entities
{
    using System;
    using System.Net.Cache;
    using System.Security.Cryptography;

    /// <summary>
    /// An helper class
    /// </summary>
    public static class Helpers
    {
        public static class RegexHelper
        {
            public const string ConfirmRegex = "\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b";
            public const string NoRegex = "[Nn][Oo]?";
            public const string YesRegex = "[Yy][Ee]?[Ss]?";
        }

        public static class RandomGenerator
        {
            private static readonly RNGCryptoServiceProvider Rand = new RNGCryptoServiceProvider();

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
    }
}