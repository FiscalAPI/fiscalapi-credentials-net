using System;
using System.Text;

namespace Fiscalapi.Credentials.Common
{
    public static class StringExtensions
    {
        /// <summary>
        /// Encode string to base64
        /// </summary>
        /// <param name="plainText">string to encode</param>
        /// <returns>base64 encoded string</returns>
        public static string EncodeToBase64(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decode string to plainText
        /// </summary>
        /// <param name="base64EncodedData">base64 encoded data to decode</param>
        /// <returns>plainText</returns>
        public static string DecodeFromBase64(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Get array bytes from any string
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns>array bytes</returns>
        public static byte[] GetBytes(this string plainText)
        {
            return Encoding.UTF8.GetBytes(plainText);
        }


        /// <summary>
        /// Converts an array of bytes to base64 encode
        /// </summary>
        /// <param name="inArray"></param>
        /// <returns>base64String</returns>
        public static string ToBase64String(this byte[] inArray)
        {
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// Get an array of bytes of base64 encoded text
        /// </summary>
        /// <param name="base64EncodedText"></param>
        /// <returns></returns>
        public static byte[] FromBase64String(this string base64EncodedText)
        {
            return Convert.FromBase64String(base64EncodedText);
        }
    }
}