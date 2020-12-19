using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SteamTool.Proxy
{
    public static class CertificateEx
    {
        public static string GetPublicPemCertificateString(this X509Certificate2 @this)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(Const.HOST_TAG);
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(
                Convert.ToBase64String(@this.RawData, Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");
            builder.AppendLine(Const.HOST_TAG);

            return builder.ToString();
        }
    }
}
