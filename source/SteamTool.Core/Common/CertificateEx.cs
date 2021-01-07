using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SteamTool.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

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

        public static string GetPrivatePemCertificateString(this X509Certificate2 @this)
        {
            RSACryptoServiceProvider pkey = (RSACryptoServiceProvider)@this.PrivateKey;

            AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetRsaKeyPair(pkey);
            using TextWriter tw = new StreamWriter(Path.Combine(AppContext.BaseDirectory, @this.IssuerName.Name + ".key"));
            PemWriter pw = new PemWriter(tw);
            pw.WriteObject(keyPair.Private);
            tw.Flush();
            return tw.ToString();
        }

        public static List<string> GetSubjectAlternativeNames(this X509Certificate2 certificate)
        {
            foreach (X509Extension extension in certificate.Extensions)
            {
                // Create an AsnEncodedData object using the extensions information.
                AsnEncodedData asndata = new AsnEncodedData(extension.Oid, extension.RawData);
                if (string.Equals(extension.Oid.FriendlyName, "Subject Alternative Name"))
                {
                    //Console.WriteLine("Extension type: {0}", extension.Oid.FriendlyName);
                    //Console.WriteLine("Oid value: {0}", asndata.Oid.Value);
                    //Console.WriteLine("Raw data length: {0} {1}", asndata.RawData.Length, Environment.NewLine);
                    //Console.WriteLine(asndata.Format(true));

                    return new List<string>(
                        asndata.Format(true).Split(new string[] { Environment.NewLine, "DNS Name=" },
                             StringSplitOptions.RemoveEmptyEntries));
                }

            }
            return new List<string>();
        }


        public static void SaveCerCertificateFile(this X509Certificate2 @this, string pathOrName)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(
                Convert.ToBase64String(@this.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");

            File.WriteAllText(pathOrName, builder.ToString(), Encoding.UTF8);
        }
    }
}
