using System.Collections.Generic;

namespace Titanium.Web.Proxy.Http
{
    class InternalDataStore : Dictionary<string, object>
    {
        public bool TryGetValueAs<T>(string key, out T value)
        {
            bool result = TryGetValue(key, out var value1);
            if (result)
            {
                value = (T)value1;
            }
            else
            {
                // hack: https://stackoverflow.com/questions/54593923/nullable-reference-types-with-generic-return-type
                value = default!;
            }

            return result;
        }

        public T GetAs<T>(string key)
        {
            return (T)this[key];
        }
    }
}
