using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Http
{
    /// <summary>
    /// The http header collection.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class HeaderCollection : IEnumerable<HttpHeader>
    {
        private readonly Dictionary<string, HttpHeader> headers;

        private readonly Dictionary<string, List<HttpHeader>> nonUniqueHeaders;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HeaderCollection" /> class.
        /// </summary>
        public HeaderCollection()
        {
            headers = new Dictionary<string, HttpHeader>(StringComparer.OrdinalIgnoreCase);
            nonUniqueHeaders = new Dictionary<string, List<HttpHeader>>(StringComparer.OrdinalIgnoreCase);
            Headers = new ReadOnlyDictionary<string, HttpHeader>(headers);
            NonUniqueHeaders = new ReadOnlyDictionary<string, List<HttpHeader>>(nonUniqueHeaders);
        }

        /// <summary>
        ///     Unique Request header collection.
        /// </summary>
        public ReadOnlyDictionary<string, HttpHeader> Headers { get; }

        /// <summary>
        ///     Non Unique headers.
        /// </summary>
        public ReadOnlyDictionary<string, List<HttpHeader>> NonUniqueHeaders { get; }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<HttpHeader> GetEnumerator()
        {
            return headers.Values.Concat(nonUniqueHeaders.Values.SelectMany(x => x)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     True if header exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HeaderExists(string name)
        {
            return headers.ContainsKey(name) || nonUniqueHeaders.ContainsKey(name);
        }

        /// <summary>
        ///     Returns all headers with given name if exists
        ///     Returns null if doesn't exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<HttpHeader>? GetHeaders(string name)
        {
            if (headers.ContainsKey(name))
            {
                return new List<HttpHeader>
                {
                    headers[name]
                };
            }

            if (nonUniqueHeaders.ContainsKey(name))
            {
                return new List<HttpHeader>(nonUniqueHeaders[name]);
            }

            return null;
        }

        public HttpHeader? GetFirstHeader(string name)
        {
            if (headers.TryGetValue(name, out var header))
            {
                return header;
            }

            if (nonUniqueHeaders.TryGetValue(name, out var h))
            {
                return h.FirstOrDefault();
            }

            return null;
        }

        internal HttpHeader? GetFirstHeader(KnownHeader name)
        {
            if (headers.TryGetValue(name.String, out var header))
            {
                return header;
            }

            if (nonUniqueHeaders.TryGetValue(name.String, out var h))
            {
                return h.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        ///     Returns all headers
        /// </summary>
        /// <returns></returns>
        public List<HttpHeader> GetAllHeaders()
        {
            var result = new List<HttpHeader>();

            result.AddRange(headers.Select(x => x.Value));
            result.AddRange(nonUniqueHeaders.SelectMany(x => x.Value));

            return result;
        }

        /// <summary>
        ///     Add a new header with given name and value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeader(string name, string value)
        {
            AddHeader(new HttpHeader(name, value));
        }

        internal void AddHeader(KnownHeader name, string value)
        {
            AddHeader(new HttpHeader(name, value));
        }

        internal void AddHeader(KnownHeader name, KnownHeader value)
        {
            AddHeader(new HttpHeader(name, value));
        }

        /// <summary>
        ///     Adds the given header object to Request
        /// </summary>
        /// <param name="newHeader"></param>
        public void AddHeader(HttpHeader newHeader)
        {
            // if header exist in non-unique header collection add it there
            if (nonUniqueHeaders.TryGetValue(newHeader.Name, out var list))
            {
                list.Add(newHeader);
                return;
            }

            // if header is already in unique header collection then move both to non-unique collection
            if (headers.TryGetValue(newHeader.Name, out var existing))
            {
                headers.Remove(newHeader.Name);

                nonUniqueHeaders.Add(newHeader.Name, new List<HttpHeader>
                {
                    existing,
                    newHeader
                });
            }
            else
            {
                // add to unique header collection
                headers.Add(newHeader.Name, newHeader);
            }
        }

        /// <summary>
        ///     Adds the given header objects to Request
        /// </summary>
        /// <param name="newHeaders"></param>
        public void AddHeaders(IEnumerable<HttpHeader>? newHeaders)
        {
            if (newHeaders == null)
            {
                return;
            }

            foreach (var header in newHeaders)
            {
                AddHeader(header);
            }
        }

        /// <summary>
        ///     Adds the given header objects to Request
        /// </summary>
        /// <param name="newHeaders"></param>
        public void AddHeaders(IEnumerable<KeyValuePair<string, string>> newHeaders)
        {
            if (newHeaders == null)
            {
                return;
            }

            foreach (var header in newHeaders)
            {
                AddHeader(header.Key, header.Value);
            }
        }

        /// <summary>
        ///     Adds the given header objects to Request
        /// </summary>
        /// <param name="newHeaders"></param>
        public void AddHeaders(IEnumerable<KeyValuePair<string, HttpHeader>>? newHeaders)
        {
            if (newHeaders == null)
            {
                return;
            }

            foreach (var header in newHeaders)
            {
                if (header.Key != header.Value.Name)
                {
                    throw new Exception(
                        "Header name mismatch. Key and the name of the HttpHeader object should be the same.");
                }

                AddHeader(header.Value);
            }
        }

        /// <summary>
        ///     removes all headers with given name
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns>
        ///     True if header was removed
        ///     False if no header exists with given name
        /// </returns>
        public bool RemoveHeader(string headerName)
        {
            bool result = headers.Remove(headerName);

            // do not convert to '||' expression to avoid lazy evaluation
            if (nonUniqueHeaders.Remove(headerName))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        ///     removes all headers with given name
        /// </summary>
        /// <param name="headerName"></param>
        /// <returns>
        ///     True if header was removed
        ///     False if no header exists with given name
        /// </returns>
        public bool RemoveHeader(KnownHeader headerName)
        {
            bool result = headers.Remove(headerName.String);

            // do not convert to '||' expression to avoid lazy evaluation
            if (nonUniqueHeaders.Remove(headerName.String))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        ///     Removes given header object if it exist
        /// </summary>
        /// <param name="header">Returns true if header exists and was removed </param>
        public bool RemoveHeader(HttpHeader header)
        {
            if (headers.ContainsKey(header.Name))
            {
                if (headers[header.Name].Equals(header))
                {
                    headers.Remove(header.Name);
                    return true;
                }
            }
            else if (nonUniqueHeaders.ContainsKey(header.Name))
            {
                if (nonUniqueHeaders[header.Name].RemoveAll(x => x.Equals(header)) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Removes all the headers.
        /// </summary>
        public void Clear()
        {
            headers.Clear();
            nonUniqueHeaders.Clear();
        }

        internal string? GetHeaderValueOrNull(KnownHeader headerName)
        {
            if (headers.TryGetValue(headerName.String, out var header))
            {
                return header.Value;
            }

            return null;
        }

        internal void SetOrAddHeaderValue(KnownHeader headerName, string? value)
        {
            if (value == null)
            {
                RemoveHeader(headerName);
                return;
            }

            if (headers.TryGetValue(headerName.String, out var header))
            {
                header.SetValue(value);
            }
            else
            {
                headers.Add(headerName.String, new HttpHeader(headerName, value));
            }
        }

        internal void SetOrAddHeaderValue(KnownHeader headerName, KnownHeader value)
        {
            if (headers.TryGetValue(headerName.String, out var header))
            {
                header.SetValue(value);
            }
            else
            {
                headers.Add(headerName.String, new HttpHeader(headerName, value));
            }
        }

        /// <summary>
        ///     Fix proxy specific headers
        /// </summary>
        internal void FixProxyHeaders()
        {
            // If proxy-connection close was returned inform to close the connection
            string? proxyHeader = GetHeaderValueOrNull(KnownHeaders.ProxyConnection);
            RemoveHeader(KnownHeaders.ProxyConnection);

            if (proxyHeader != null)
            {
                SetOrAddHeaderValue(KnownHeaders.Connection, proxyHeader);
            }
        }
    }
}
