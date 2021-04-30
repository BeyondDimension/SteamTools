using MessagePack;
using System.Application.Properties;
using System.Collections.Generic;

namespace System.Application.Models
{
    partial class OpenSourceLibrary
    {
        static readonly Lazy<List<OpenSourceLibrary>> mValues = new(() =>
        {
            try
            {
                var list = Serializable.DMP<List<OpenSourceLibrary>>(SR.OpenSourceLibraryList) ?? new List<OpenSourceLibrary>();
                return list;
            }
            catch
            {
                return new List<OpenSourceLibrary>();
            }
        });

        public static List<OpenSourceLibrary> Values => mValues.Value;

        static readonly Lazy<string> mStringValues = new(() => ToString(Values));

        public static string StringValues => mStringValues.Value;
    }
}