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

using System;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Http2.Hpack
{
    public class DynamicTable
    {
        // a circular queue of header fields
#if NET45
        HttpHeader[] headerFields = new HttpHeader[0];
#else
        HttpHeader[] headerFields = Array.Empty<HttpHeader>();
#endif
        int head;
        int tail;

        /// <summary>
        /// Return the maximum allowable size of the dynamic table.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        // ensure setCapacity creates the array
        public int Capacity { get; private set; } = -1;

        /// <summary>
        /// Return the current size of the dynamic table.
        /// This is the sum of the size of the entries.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size { get; private set; }

        /// <summary>
        /// Creates a new dynamic table with the specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity.</param>
        public DynamicTable(int initialCapacity)
        {
            SetCapacity(initialCapacity);
        }

        /// <summary>
        /// Return the number of header fields in the dynamic table.
        /// </summary>
        public int Length()
        {
            int length;
            if (head < tail)
            {
                length = headerFields.Length - tail + head;
            }
            else
            {
                length = head - tail;
            }

            return length;
        }

        /// <summary>
        /// Return the header field at the given index.
        /// The first and newest entry is always at index 1,
        /// and the oldest entry is at the index length().
        /// </summary>
        /// <returns>The entry.</returns>
        /// <param name="index">Index.</param>
        public HttpHeader GetEntry(int index)
        {
            if (index <= 0 || index > Length())
            {
                throw new IndexOutOfRangeException();
            }

            int i = head - index;
            if (i < 0)
            {
                return headerFields[i + headerFields.Length]!;
            }

            return headerFields[i]!;
        }

        /// <summary>
        /// Add the header field to the dynamic table.
        /// Entries are evicted from the dynamic table until the size of the table
        /// and the new header field is less than or equal to the table's capacity.
        /// If the size of the new entry is larger than the table's capacity,
        /// the dynamic table will be cleared.
        /// </summary>
        /// <param name="header">Header.</param>
        public void Add(HttpHeader header)
        {
            int headerSize = header.Size;
            if (headerSize > Capacity)
            {
                Clear();
                return;
            }

            while (Size + headerSize > Capacity)
            {
                Remove();
            }

            headerFields[head++] = header;
            Size += header.Size;
            if (head == headerFields.Length)
            {
                head = 0;
            }
        }

        /// <summary>
        /// Remove and return the oldest header field from the dynamic table.
        /// </summary>
        public HttpHeader? Remove()
        {
            var removed = headerFields[tail];
            if (removed == null)
            {
                return null;
            }

            Size -= removed.Size;
            headerFields[tail++] = null!;
            if (tail == headerFields.Length)
            {
                tail = 0;
            }

            return removed;
        }

        /// <summary>
        /// Remove all entries from the dynamic table.
        /// </summary>
        public void Clear()
        {
            while (tail != head)
            {
                headerFields[tail++] = null!;
                if (tail == headerFields.Length)
                {
                    tail = 0;
                }
            }

            head = 0;
            tail = 0;
            Size = 0;
        }

        /// <summary>
        /// Set the maximum size of the dynamic table.
        /// Entries are evicted from the dynamic table until the size of the table
        /// is less than or equal to the maximum size.
        /// </summary>
        /// <param name="capacity">Capacity.</param>
        public void SetCapacity(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("Illegal Capacity: " + capacity);
            }

            // initially capacity will be -1 so init won't return here
            if (Capacity == capacity)
            {
                return;
            }

            Capacity = capacity;

            if (capacity == 0)
            {
                Clear();
            }
            else
            {
                // initially size will be 0 so remove won't be called
                while (Size > capacity)
                {
                    Remove();
                }
            }

            int maxEntries = capacity / HttpHeader.HttpHeaderOverhead;
            if (capacity % HttpHeader.HttpHeaderOverhead != 0)
            {
                maxEntries++;
            }

            // check if capacity change requires us to reallocate the array
            if (headerFields != null && headerFields.Length == maxEntries)
            {
                return;
            }

            var tmp = new HttpHeader[maxEntries];

            // initially length will be 0 so there will be no copy
            int len = Length();
            int cursor = tail;
            for (int i = 0; i < len; i++)
            {
                var entry = headerFields![cursor++];
                tmp[i] = entry!;
                if (cursor == headerFields.Length)
                {
                    cursor = 0;
                }
            }

            tail = 0;
            head = tail + len;
            headerFields = tmp;
        }
    }
}
