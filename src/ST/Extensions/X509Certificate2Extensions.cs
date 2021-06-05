using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    public static class X509Certificate2Extensions
    {
        public static string GetPublicPemCertificateString(this X509Certificate2 @this)
        {
            StringBuilder builder = new();
            //builder.AppendLine(Constants.CERTIFICATE_TAG);
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(
                Convert.ToBase64String(@this.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");
            //builder.AppendLine(Constants.CERTIFICATE_TAG);
            return builder.ToString();
        }

        //[Obsolete("warn AppContext.BaseDirectory ?", true)]
        //public static string GetPrivatePemCertificateString(this X509Certificate2 @this)
        //{
        //    if (@this.PrivateKey is RSACryptoServiceProvider pkey)
        //    {
        //        var keyPair = DotNetUtilities.GetRsaKeyPair(pkey);
        //        using TextWriter tw = new StreamWriter(Path.Combine(IOPath.BaseDirectory, @this.IssuerName.Name + ".key"), false, new UTF8Encoding(false));
        //        PemWriter pw = new PemWriter(tw);
        //        pw.WriteObject(keyPair.Private);
        //        tw.Flush();
        //        return tw.ToString();
        //    }
        //    else
        //    {
        //        throw new InvalidCastException(nameof(X509Certificate2.PrivateKey));
        //    }
        //}

        public static List<string> GetSubjectAlternativeNames(this X509Certificate2 certificate)
        {
            foreach (X509Extension extension in certificate.Extensions)
            {
                // Create an AsnEncodedData object using the extensions information.
                AsnEncodedData asndata = new AsnEncodedData(extension.Oid, extension.RawData);
                if (string.Equals(extension.Oid.FriendlyName, "Subject Alternative Name"))
                {
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
            File.WriteAllText(pathOrName, builder.ToString(), new UTF8Encoding(false));
        }
    }
}