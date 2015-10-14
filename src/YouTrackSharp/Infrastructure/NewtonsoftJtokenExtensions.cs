using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RestSharp.Extensions;

namespace YouTrackSharp.Infrastructure
{
    static class NewtonsoftJtokenExtensions
    {
        /// <summary>
        /// Gets string value from JToken
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string AsString(this JToken token)
        {
            return token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
        }

        /// <summary>
        /// Gets string value from JToken
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string AsString(this JToken token, CultureInfo culture)
        {
            var value = token as JValue;
            if (value != null)
            {
                return (string)value.Value.ChangeType(typeof(string), culture);
            }

            return token.Type == JTokenType.String ? token.Value<string>() : token.ToString();


        }

    }
}
