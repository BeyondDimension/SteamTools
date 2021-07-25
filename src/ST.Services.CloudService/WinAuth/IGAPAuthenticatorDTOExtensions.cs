using System;
using System.Application.Models;
using System.Text.RegularExpressions;
using System.Web;
using static System.Application.Models.GAPAuthenticatorValueDTO;
using Base32 = WinAuth.WinAuthBase32;

// ReSharper disable once CheckNamespace
namespace WinAuth
{
    public static class GAPAuthenticatorDTOExtensions
    {
        /// <summary>
        /// Create a KeyUriFormat compatible URL
        /// See https://code.google.com/p/google-authenticator/wiki/KeyUriFormat
        /// </summary>
        /// <param name="this"></param>
        /// <param name="compat"></param>
        /// <returns></returns>
        public static string ToUrl(this IGAPAuthenticatorDTO @this, bool compat = false)
        {
            string type = "totp";
            string extraparams = string.Empty;

            Match match;
            var issuer = @this.Value.Issuer;
            var label = @this.Name;
            if (string.IsNullOrEmpty(issuer) == true && (match = Regex.Match(label, @"^([^\(]+)\s+\((.*?)\)(.*)")).Success == true)
            {
                issuer = match.Groups[1].Value;
                label = match.Groups[2].Value + match.Groups[3].Value;
            }
            if (string.IsNullOrEmpty(issuer) == false && (match = Regex.Match(label, @"^" + issuer + @"\s+\((.*?)\)(.*)")).Success == true)
            {
                label = match.Groups[1].Value + match.Groups[2].Value;
            }
            if (string.IsNullOrEmpty(issuer) == false)
            {
                extraparams += "&issuer=" + HttpUtility.UrlEncode(issuer);
            }

            if (@this.Value.HMACType != DEFAULT_HMAC_TYPE)
            {
                extraparams += "&algorithm=" + @this.Value.HMACType.ToString();
            }

            if (@this.Value is BattleNetAuthenticator battleNetAuthenticator)
            {
                extraparams += "&serial=" + HttpUtility.UrlEncode(battleNetAuthenticator.Serial?.Replace("-", ""));
            }
            else if (@this.Value is SteamAuthenticator steamAuthenticator)
            {
                if (!compat)
                {
                    extraparams += "&deviceid=" + HttpUtility.UrlEncode(steamAuthenticator.DeviceId);
                    extraparams += "&data=" + HttpUtility.UrlEncode(steamAuthenticator.SteamData);
                }
            }
            else if (@this.Value is HOTPAuthenticator hOTPAuthenticator)
            {
                type = "hotp";
                extraparams += "&counter=" + hOTPAuthenticator.Counter;
            }

            string secret = HttpUtility.UrlEncode(Base32.GetInstance().Encode(@this.Value.SecretKey ?? Array.Empty<byte>()));

            if (@this.Value.Period != DEFAULT_PERIOD)
            {
                extraparams += "&period=" + @this.Value.Period;
            }

            var url = string.Format("otpauth://" + type + "/{0}?secret={1}&digits={2}{3}",
              (string.IsNullOrEmpty(issuer) == false ? HttpUtility.UrlPathEncode(issuer) + ":" + HttpUtility.UrlPathEncode(label) : HttpUtility.UrlPathEncode(label)),
              secret,
              @this.Value.CodeDigits,
              extraparams);

            return url;
        }
    }
}