using System.Application.Properties;
using System.Collections.Generic;

namespace System.Application.Models
{
    partial class OpenSourceLibrary
    {
        static readonly Lazy<List<OpenSourceLibrary>> mValues = new Lazy<List<OpenSourceLibrary>>(() =>
        {
            try
            {
                return Serializable.DMP<List<OpenSourceLibrary>>(SR.OpenSourceLibraryList) ?? new List<OpenSourceLibrary>();
            }
            catch
            {
                return new List<OpenSourceLibrary>();
            }
        });

        public static List<OpenSourceLibrary> Values => mValues.Value;

        static readonly Lazy<string> mStringValues = new Lazy<string>(() => ToString(Values));

        public static string StringValues => mStringValues.Value;
    }
}