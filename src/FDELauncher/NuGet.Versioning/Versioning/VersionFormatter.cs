// https://github.com/NuGetArchive/NuGet.Versioning/blob/release/src/Versioning/VersionFormatter.cs

using System;
using System.Globalization;
using System.Text;

namespace NuGet.Versioning
{
    public class VersionFormatter : IFormatProvider, ICustomFormatter
    {
        public string? Format(string? format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }

            string? formatted = null;
            Type argType = arg.GetType();

            if (argType == typeof(IFormattable))
            {
                formatted = ((IFormattable)arg).ToString(format, formatProvider);
            }
            else if (!string.IsNullOrEmpty(format))
            {
                var version = arg as SemanticVersion;

                if (version != null)
                {
                    // single char identifiers
                    if (format!.Length == 1)
                    {
                        formatted = Format(format[0], version);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder(format.Length);

                        for (int i = 0; i < format.Length; i++)
                        {
                            var s = Format(format[i], version);

                            if (s == null)
                            {
                                sb.Append(format[i]);
                            }
                            else
                            {
                                sb.Append(s);
                            }
                        }

                        formatted = sb.ToString();
                    }
                }
            }

            return formatted;
        }

        public object? GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)
                || formatType == typeof(NuGetVersion)
                || formatType == typeof(SemanticVersion))
            {
                return this;
            }

            return null;
        }

        private static string GetNormalizedString(SemanticVersion version)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Format('V', version));

            if (version.IsPrerelease)
            {
                sb.Append('-');
                sb.Append(version.Release);
            }

            if (version.HasMetadata)
            {
                sb.Append('+');
                sb.Append(version.Metadata);
            }

            return sb.ToString();
        }

        private static string? Format(char c, SemanticVersion version)
        {
            string? s = null;

            switch (c)
            {
                case 'N':
                    s = GetNormalizedString(version);
                    break;
                case 'R':
                    s = version.Release;
                    break;
                case 'M':
                    s = version.Metadata;
                    break;
                case 'V':
                    s = FormatVersion(version);
                    break;
                case 'x':
                    s = string.Format(CultureInfo.InvariantCulture, "{0}", version.Major);
                    break;
                case 'y':
                    s = string.Format(CultureInfo.InvariantCulture, "{0}", version.Minor);
                    break;
                case 'z':
                    s = string.Format(CultureInfo.InvariantCulture, "{0}", version.Patch);
                    break;
                case 'r':
                    var nuGetVersion = version as NuGetVersion;
                    s = string.Format(CultureInfo.InvariantCulture, "{0}", nuGetVersion != null && nuGetVersion.IsLegacyVersion ? nuGetVersion.Version.Revision : 0);
                    break;
            }

            return s;
        }

        private static string FormatVersion(SemanticVersion version)
        {
            var nuGetVersion = version as NuGetVersion;
            bool legacy = nuGetVersion != null && nuGetVersion.IsLegacyVersion;

            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}{3}", version.Major, version.Minor, version.Patch,
                legacy ? string.Format(CultureInfo.InvariantCulture, ".{0}", nuGetVersion!.Version.Revision) : null);
        }
    }
}