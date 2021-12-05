using System.Application.Properties;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Models
{
    partial class OpenSourceLibrary
    {
        static readonly Lazy<List<OpenSourceLibrary>> mValues = new(() =>
        {
            try
            {
                var list = Serializable.DMP<List<OpenSourceLibrary>>(SR.OpenSourceLibraryList);
                if (list != null)
                {
                    var ignoreItem = list.Where(x => ignoreList.Contains(x.Name)).ToArray();
                    Array.ForEach(ignoreItem, x => list.Remove(x));
                    return list;
                }
            }
            catch
            {
            }
            return new();
        });

        public static List<OpenSourceLibrary> Values => mValues.Value;

        static readonly Lazy<string> mStringValues = new(() => ToString(Values));

        public static string StringValues => mStringValues.Value;
    }
}