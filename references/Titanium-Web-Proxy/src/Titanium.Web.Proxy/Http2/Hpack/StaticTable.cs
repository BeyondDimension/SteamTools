/*
 * Copyright 2014 Twitter, Inc
 * This file is a derivative work modified by Ringo Leese
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Http2.Hpack
{
    internal static class StaticTable
    {
        /// <summary>
        /// Appendix A: Static Table Definition
        /// </summary>
        /// <see cref="http://tools.ietf.org/html/rfc7541#appendix-A"/>
        private static readonly List<HttpHeader> staticTable;

        private static readonly Dictionary<ByteString, int> staticIndexByName;

        public static ByteString KnownHeaderAuhtority = (ByteString)":authority";

        public static ByteString KnownHeaderMethod = (ByteString)":method";

        public static ByteString KnownHeaderPath = (ByteString)":path";

        public static ByteString KnownHeaderScheme = (ByteString)":scheme";

        public static ByteString KnownHeaderStatus = (ByteString)":status";

        static StaticTable()
        {
            const int entryCount = 61;
            staticTable = new List<HttpHeader>(entryCount);
            staticIndexByName = new Dictionary<ByteString, int>(entryCount);
            create(KnownHeaderAuhtority, string.Empty); // 1
            create(KnownHeaderMethod, "GET"); // 2
            create(KnownHeaderMethod, "POST"); // 3
            create(KnownHeaderPath, "/"); // 4
            create(KnownHeaderPath, "/index.html"); // 5
            create(KnownHeaderScheme, "http"); // 6
            create(KnownHeaderScheme, "https"); // 7
            create(KnownHeaderStatus, "200"); // 8
            create(KnownHeaderStatus, "204"); // 9
            create(KnownHeaderStatus, "206"); // 10
            create(KnownHeaderStatus, "304"); // 11
            create(KnownHeaderStatus, "400"); // 12
            create(KnownHeaderStatus, "404"); // 13
            create(KnownHeaderStatus, "500"); // 14
            create("Accept-Charset", string.Empty); // 15
            create("Accept-Encoding", "gzip, deflate"); // 16
            create("Accept-Language", string.Empty); // 17
            create("Accept-Ranges", string.Empty); // 18
            create("Accept", string.Empty); // 19
            create("Access-Control-Allow-Origin", string.Empty); // 20
            create("Age", string.Empty); // 21
            create("Allow", string.Empty); // 22
            create("Authorization", string.Empty); // 23
            create("Cache-Control", string.Empty); // 24
            create("Content-Disposition", string.Empty); // 25
            create("Content-Encoding", string.Empty); // 26
            create("Content-Language", string.Empty); // 27
            create("Content-Length", string.Empty); // 28
            create("Content-Location", string.Empty); // 29
            create("Content-Range", string.Empty); // 30
            create("Content-Type", string.Empty); // 31
            create("Cookie", string.Empty); // 32
            create("Date", string.Empty); // 33
            create("ETag", string.Empty); // 34
            create("Expect", string.Empty); // 35
            create("Expires", string.Empty); // 36
            create("From", string.Empty); // 37
            create("Host", string.Empty); // 38
            create("If-Match", string.Empty); // 39
            create("If-Modified-Since", string.Empty); // 40
            create("If-None-Match", string.Empty); // 41
            create("If-Range", string.Empty); // 42
            create("If-Unmodified-Since", string.Empty); // 43
            create("Last-Modified", string.Empty); // 44
            create("Link", string.Empty); // 45
            create("Location", string.Empty); // 46
            create("Max-Forwards", string.Empty); // 47
            create("Proxy-Authenticate", string.Empty); // 48
            create("Proxy-Authorization", string.Empty); // 49
            create("Range", string.Empty); // 50
            create("Referer", string.Empty); // 51
            create("Refresh", string.Empty); // 52
            create("Retry-After", string.Empty); // 53
            create("Server", string.Empty); // 54
            create("Set-Cookie", string.Empty); // 55
            create("Strict-Transport-Security", string.Empty); // 56
            create("Transfer-Encoding", string.Empty); // 57
            create("User-Agent", string.Empty); // 58
            create("Vary", string.Empty); // 59
            create("Via", string.Empty); // 60
            create("WWW-Authenticate", string.Empty); // 61
        }

        /// <summary>
        /// The number of header fields in the static table.
        /// </summary>
        /// <value>The length.</value>
        public static int Length => staticTable.Count;

        /// <summary>
        /// Return the http header field at the given index value.
        /// </summary>
        /// <returns>The header field.</returns>
        /// <param name="index">Index.</param>
        public static HttpHeader Get(int index)
        {
            return staticTable[index - 1];
        }

        /// <summary>
        /// Returns the lowest index value for the given header field name in the static table.
        /// Returns -1 if the header field name is not in the static table.
        /// </summary>
        /// <returns>The index.</returns>
        /// <param name="name">Name.</param>
        public static int GetIndex(ByteString name)
        {
            if (!staticIndexByName.TryGetValue(name, out int index))
            {
                return -1;
            }

            return index;
        }

        /// <summary>
        /// Returns the index value for the given header field in the static table.
        /// Returns -1 if the header field is not in the static table.
        /// </summary>
        /// <returns>The index.</returns>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public static int GetIndex(ByteString name, ByteString value)
        {
            int index = GetIndex(name);
            if (index == -1)
            {
                return -1;
            }

            // Note this assumes all entries for a given header field are sequential.
            while (index <= Length)
            {
                var entry = Get(index);
                if (!name.Equals(entry.NameData))
                {
                    break;
                }

                if (Equals(value, entry.Value))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        private static void create(string name, string value)
        {
            create((ByteString)name.ToLower(), value);
        }

        private static void create(ByteString name, string value)
        {
            staticTable.Add(new HttpHeader(name, (ByteString)value));
            staticIndexByName[name] = staticTable.Count;
        }
    }
}
