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
using System.IO;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Http2.Hpack
{
    internal class Decoder
    {
        private readonly DynamicTable dynamicTable;

        private readonly int maxHeaderSize;
        private int maxDynamicTableSize;
        private int encoderMaxDynamicTableSize;
        private bool maxDynamicTableSizeChangeRequired;

        private long headerSize;
        private State state;
        private HpackUtil.IndexType indexType;
        private int index;
        private bool huffmanEncoded;
        private int skipLength;
        private int nameLength;
        private int valueLength;
        private ByteString name;

        private enum State
        {
            ReadHeaderRepresentation,
            ReadMaxDynamicTableSize,
            ReadIndexedHeader,
            ReadIndexedHeaderName,
            ReadLiteralHeaderNameLengthPrefix,
            ReadLiteralHeaderNameLength,
            ReadLiteralHeaderName,
            SkipLiteralHeaderName,
            ReadLiteralHeaderValueLengthPrefix,
            ReadLiteralHeaderValueLength,
            ReadLiteralHeaderValue,
            SkipLiteralHeaderValue
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Decoder"/> class.
        /// </summary>
        /// <param name="maxHeaderSize">Max header size.</param>
        /// <param name="maxHeaderTableSize">Max header table size.</param>
        public Decoder(int maxHeaderSize, int maxHeaderTableSize)
        {
            dynamicTable = new DynamicTable(maxHeaderTableSize);
            this.maxHeaderSize = maxHeaderSize;
            maxDynamicTableSize = maxHeaderTableSize;
            encoderMaxDynamicTableSize = maxHeaderTableSize;
            maxDynamicTableSizeChangeRequired = false;
            Reset();
        }

        private void Reset()
        {
            headerSize = 0;
            state = State.ReadHeaderRepresentation;
            indexType = HpackUtil.IndexType.None;
        }

        /// <summary>
        /// Decode the header block into header fields.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="headerListener">Header listener.</param>
        public void Decode(BinaryReader input, IHeaderListener headerListener)
        {
            while (input.BaseStream.Length - input.BaseStream.Position > 0)
            {
                switch (state)
                {
                    case State.ReadHeaderRepresentation:
                        sbyte b = input.ReadSByte();
                        if (maxDynamicTableSizeChangeRequired && (b & 0xE0) != 0x20)
                        {
                            // Encoder MUST signal maximum dynamic table size change
                            throw new IOException("max dynamic table size change required");
                        }

                        if (b < 0)
                        {
                            // Indexed Header Field
                            index = b & 0x7F;
                            if (index == 0)
                            {
                                throw new IOException("illegal index value (" + index + ")");
                            }
                            else if (index == 0x7F)
                            {
                                state = State.ReadIndexedHeader;
                            }
                            else
                            {
                                IndexHeader(index, headerListener);
                            }
                        }
                        else if ((b & 0x40) == 0x40)
                        {
                            // Literal Header Field with Incremental Indexing
                            indexType = HpackUtil.IndexType.Incremental;
                            index = b & 0x3F;
                            if (index == 0)
                            {
                                state = State.ReadLiteralHeaderNameLengthPrefix;
                            }
                            else if (index == 0x3F)
                            {
                                state = State.ReadIndexedHeaderName;
                            }
                            else
                            {
                                // Index was stored as the prefix
                                ReadName(index);
                                state = State.ReadLiteralHeaderValueLengthPrefix;
                            }
                        }
                        else if ((b & 0x20) == 0x20)
                        {
                            // Dynamic Table Size Update
                            index = b & 0x1F;
                            if (index == 0x1F)
                            {
                                state = State.ReadMaxDynamicTableSize;
                            }
                            else
                            {
                                SetDynamicTableSize(index);
                                state = State.ReadHeaderRepresentation;
                            }
                        }
                        else
                        {
                            // Literal Header Field without Indexing / never Indexed
                            indexType = ((b & 0x10) == 0x10) ? HpackUtil.IndexType.Never : HpackUtil.IndexType.None;
                            index = b & 0x0F;
                            if (index == 0)
                            {
                                state = State.ReadLiteralHeaderNameLengthPrefix;
                            }
                            else if (index == 0x0F)
                            {
                                state = State.ReadIndexedHeaderName;
                            }
                            else
                            {
                                // Index was stored as the prefix
                                ReadName(index);
                                state = State.ReadLiteralHeaderValueLengthPrefix;
                            }
                        }

                        break;

                    case State.ReadMaxDynamicTableSize:
                        int maxSize = decodeULE128(input);
                        if (maxSize == -1)
                        {
                            return;
                        }

                        // Check for numerical overflow
                        if (maxSize > int.MaxValue - index)
                        {
                            throw new IOException("decompression failure");
                        }

                        SetDynamicTableSize(index + maxSize);
                        state = State.ReadHeaderRepresentation;
                        break;

                    case State.ReadIndexedHeader:
                        int headerIndex = decodeULE128(input);
                        if (headerIndex == -1)
                        {
                            return;
                        }

                        // Check for numerical overflow
                        if (headerIndex > int.MaxValue - index)
                        {
                            throw new IOException("decompression failure");
                        }

                        IndexHeader(index + headerIndex, headerListener);
                        state = State.ReadHeaderRepresentation;
                        break;

                    case State.ReadIndexedHeaderName:
                        // Header Name matches an entry in the Header Table
                        int nameIndex = decodeULE128(input);
                        if (nameIndex == -1)
                        {
                            return;
                        }

                        // Check for numerical overflow
                        if (nameIndex > int.MaxValue - index)
                        {
                            throw new IOException("decompression failure");
                        }

                        ReadName(index + nameIndex);
                        state = State.ReadLiteralHeaderValueLengthPrefix;
                        break;

                    case State.ReadLiteralHeaderNameLengthPrefix:
                        b = input.ReadSByte();
                        huffmanEncoded = (b & 0x80) == 0x80;
                        index = b & 0x7F;
                        if (index == 0x7f)
                        {
                            state = State.ReadLiteralHeaderNameLength;
                        }
                        else
                        {
                            nameLength = index;

                            // Disallow empty names -- they cannot be represented in HTTP/1.x
                            if (nameLength == 0)
                            {
                                throw new IOException("decompression failure");
                            }

                            // Check name length against max header size
                            if (ExceedsMaxHeaderSize(nameLength))
                            {
                                if (indexType == HpackUtil.IndexType.None)
                                {
                                    // Name is unused so skip bytes
                                    name = ByteString.Empty;
                                    skipLength = nameLength;
                                    state = State.SkipLiteralHeaderName;
                                    break;
                                }

                                // Check name length against max dynamic table size
                                if (nameLength + HttpHeader.HttpHeaderOverhead > dynamicTable.Capacity)
                                {
                                    dynamicTable.Clear();
#if NET45
                                    name = Net45Compatibility.EmptyArray;
#else
                                    name = Array.Empty<byte>();
#endif
                                    skipLength = nameLength;
                                    state = State.SkipLiteralHeaderName;
                                    break;
                                }
                            }

                            state = State.ReadLiteralHeaderName;
                        }

                        break;

                    case State.ReadLiteralHeaderNameLength:
                        // Header Name is a Literal String
                        nameLength = decodeULE128(input);
                        if (nameLength == -1)
                        {
                            return;
                        }

                        // Check for numerical overflow
                        if (nameLength > int.MaxValue - index)
                        {
                            throw new IOException("decompression failure");
                        }

                        nameLength += index;

                        // Check name length against max header size
                        if (ExceedsMaxHeaderSize(nameLength))
                        {
                            if (indexType == HpackUtil.IndexType.None)
                            {
                                // Name is unused so skip bytes
                                name = ByteString.Empty;
                                skipLength = nameLength;
                                state = State.SkipLiteralHeaderName;
                                break;
                            }

                            // Check name length against max dynamic table size
                            if (nameLength + HttpHeader.HttpHeaderOverhead > dynamicTable.Capacity)
                            {
                                dynamicTable.Clear();
                                name = ByteString.Empty;
                                skipLength = nameLength;
                                state = State.SkipLiteralHeaderName;
                                break;
                            }
                        }

                        state = State.ReadLiteralHeaderName;
                        break;

                    case State.ReadLiteralHeaderName:
                        // Wait until entire name is readable
                        if (input.BaseStream.Length - input.BaseStream.Position < nameLength)
                        {
                            return;
                        }

                        name = ReadStringLiteral(input, nameLength);
                        state = State.ReadLiteralHeaderValueLengthPrefix;
                        break;

                    case State.SkipLiteralHeaderName:

                        skipLength -= (int)input.BaseStream.Seek(skipLength, SeekOrigin.Current);
                        if (skipLength < 0)
                        {
                            skipLength = 0;
                        }

                        if (skipLength == 0)
                        {
                            state = State.ReadLiteralHeaderValueLengthPrefix;
                        }

                        break;

                    case State.ReadLiteralHeaderValueLengthPrefix:
                        b = input.ReadSByte();
                        huffmanEncoded = (b & 0x80) == 0x80;
                        index = b & 0x7F;
                        if (index == 0x7f)
                        {
                            state = State.ReadLiteralHeaderValueLength;
                        }
                        else
                        {
                            valueLength = index;

                            // Check new header size against max header size
                            long newHeaderSize1 = (long)nameLength + valueLength;
                            if (ExceedsMaxHeaderSize(newHeaderSize1))
                            {
                                // truncation will be reported during endHeaderBlock
                                headerSize = maxHeaderSize + 1;

                                if (indexType == HpackUtil.IndexType.None)
                                {
                                    // Value is unused so skip bytes
                                    state = State.SkipLiteralHeaderValue;
                                    break;
                                }

                                // Check new header size against max dynamic table size
                                if (newHeaderSize1 + HttpHeader.HttpHeaderOverhead > dynamicTable.Capacity)
                                {
                                    dynamicTable.Clear();
                                    state = State.SkipLiteralHeaderValue;
                                    break;
                                }
                            }

                            if (valueLength == 0)
                            {
#if NET45
                                InsertHeader(headerListener, name, Net45Compatibility.EmptyArray, indexType);
#else
                                name = Array.Empty<byte>();
#endif
                                state = State.ReadHeaderRepresentation;
                            }
                            else
                            {
                                state = State.ReadLiteralHeaderValue;
                            }
                        }

                        break;

                    case State.ReadLiteralHeaderValueLength:
                        // Header Value is a Literal String
                        valueLength = decodeULE128(input);
                        if (valueLength == -1)
                        {
                            return;
                        }

                        // Check for numerical overflow
                        if (valueLength > int.MaxValue - index)
                        {
                            throw new IOException("decompression failure");
                        }

                        valueLength += index;

                        // Check new header size against max header size
                        long newHeaderSize2 = (long)nameLength + valueLength;
                        if (newHeaderSize2 + headerSize > maxHeaderSize)
                        {
                            // truncation will be reported during endHeaderBlock
                            headerSize = maxHeaderSize + 1;

                            if (indexType == HpackUtil.IndexType.None)
                            {
                                // Value is unused so skip bytes
                                state = State.SkipLiteralHeaderValue;
                                break;
                            }

                            // Check new header size against max dynamic table size
                            if (newHeaderSize2 + HttpHeader.HttpHeaderOverhead > dynamicTable.Capacity)
                            {
                                dynamicTable.Clear();
                                state = State.SkipLiteralHeaderValue;
                                break;
                            }
                        }

                        state = State.ReadLiteralHeaderValue;
                        break;

                    case State.ReadLiteralHeaderValue:
                        // Wait until entire value is readable
                        if (input.BaseStream.Length - input.BaseStream.Position < valueLength)
                        {
                            return;
                        }

                        var value = ReadStringLiteral(input, valueLength);
                        InsertHeader(headerListener, name, value, indexType);
                        state = State.ReadHeaderRepresentation;
                        break;

                    case State.SkipLiteralHeaderValue:
                        valueLength -= (int)input.BaseStream.Seek(valueLength, SeekOrigin.Current);
                        if (valueLength < 0)
                        {
                            valueLength = 0;
                        }

                        if (valueLength == 0)
                        {
                            state = State.ReadHeaderRepresentation;
                        }

                        break;

                    default:
                        throw new Exception("should not reach here");
                }
            }
        }

        /// <summary>
        /// End the current header block. Returns if the header field has been truncated.
        /// This must be called after the header block has been completely decoded.
        /// </summary>
        /// <returns><c>true</c>, if header block was ended, <c>false</c> otherwise.</returns>
        public bool EndHeaderBlock()
        {
            bool truncated = headerSize > maxHeaderSize;
            Reset();
            return truncated;
        }

        /// <summary>
        /// Set the maximum table size.
        /// If this is below the maximum size of the dynamic table used by the encoder,
        /// the beginning of the next header block MUST signal this change.
        /// </summary>
        /// <param name="maxHeaderTableSize">Max header table size.</param>
        public void SetMaxHeaderTableSize(int maxHeaderTableSize)
        {
            maxDynamicTableSize = maxHeaderTableSize;
            if (maxDynamicTableSize < encoderMaxDynamicTableSize)
            {
                // decoder requires less space than encoder
                // encoder MUST signal this change
                maxDynamicTableSizeChangeRequired = true;
                dynamicTable.SetCapacity(maxDynamicTableSize);
            }
        }

        /// <summary>
        /// Return the maximum table size.
        /// This is the maximum size allowed by both the encoder and the decoder.
        /// </summary>
        /// <returns>The max header table size.</returns>
        public int GetMaxHeaderTableSize()
        {
            return dynamicTable.Capacity;
        }

        private void SetDynamicTableSize(int dynamicTableSize)
        {
            if (dynamicTableSize > maxDynamicTableSize)
            {
                throw new IOException("invalid max dynamic table size");
            }

            encoderMaxDynamicTableSize = dynamicTableSize;
            maxDynamicTableSizeChangeRequired = false;
            dynamicTable.SetCapacity(dynamicTableSize);
        }

        private HttpHeader GetHeaderField(int index)
        {
            if (index <= StaticTable.Length)
            {
                var headerField = StaticTable.Get(index);
                return headerField;
            }

            if (index - StaticTable.Length <= dynamicTable.Length())
            {
                var headerField = dynamicTable.GetEntry(index - StaticTable.Length);
                return headerField;
            }

            throw new IOException("illegal index value (" + index + ")");
        }

        private void ReadName(int index)
        {
            name = GetHeaderField(index).NameData;
        }

        private void IndexHeader(int index, IHeaderListener headerListener)
        {
            var headerField = GetHeaderField(index);
            AddHeader(headerListener, headerField.NameData, headerField.ValueData, false);
        }

        private void InsertHeader(IHeaderListener headerListener, ByteString name, ByteString value, HpackUtil.IndexType indexType)
        {
            AddHeader(headerListener, name, value, indexType == HpackUtil.IndexType.Never);

            switch (indexType)
            {
                case HpackUtil.IndexType.None:
                case HpackUtil.IndexType.Never:
                    break;

                case HpackUtil.IndexType.Incremental:
                    dynamicTable.Add(new HttpHeader(name, value));
                    break;

                default:
                    throw new Exception("should not reach here");
            }
        }

        private void AddHeader(IHeaderListener headerListener, ByteString name, ByteString value, bool sensitive)
        {
            if (name.Length == 0)
            {
                throw new ArgumentException("name is empty");
            }

            long newSize = headerSize + name.Length + value.Length;
            if (newSize <= maxHeaderSize)
            {
                headerListener.AddHeader(name, value, sensitive);
                headerSize = (int)newSize;
            }
            else
            {
                // truncation will be reported during endHeaderBlock
                headerSize = maxHeaderSize + 1;
            }
        }

        private bool ExceedsMaxHeaderSize(long size)
        {
            // Check new header size against max header size
            if (size + headerSize <= maxHeaderSize)
            {
                return false;
            }

            // truncation will be reported during endHeaderBlock
            headerSize = maxHeaderSize + 1;
            return true;
        }

        private ByteString ReadStringLiteral(BinaryReader input, int length)
        {
            var buf = new byte[length];
            int lengthToRead = length;
            if (input.BaseStream.Length - input.BaseStream.Position < length)
            {
                lengthToRead = (int)input.BaseStream.Length - (int)input.BaseStream.Position;
            }

            int readBytes = input.Read(buf, 0, lengthToRead);
            if (readBytes != length)
            {
                throw new IOException("decompression failure");
            }

            return new ByteString(huffmanEncoded ? HuffmanDecoder.Instance.Decode(buf) : buf);
        }

        // Unsigned Little Endian Base 128 Variable-Length Integer Encoding
        private static int decodeULE128(BinaryReader input)
        {
            long markedPosition = input.BaseStream.Position;
            int result = 0;
            int shift = 0;
            while (shift < 32)
            {
                if (input.BaseStream.Length - input.BaseStream.Position == 0)
                {
                    // Buffer does not contain entire integer,
                    // reset reader index and return -1.
                    input.BaseStream.Position = markedPosition;
                    return -1;
                }

                sbyte b = input.ReadSByte();
                if (shift == 28 && (b & 0xF8) != 0)
                {
                    break;
                }

                result |= (b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    return result;
                }

                shift += 7;
            }

            // Value exceeds Integer.MAX_VALUE
            input.BaseStream.Position = markedPosition;
            throw new IOException("decompression failure");
        }
    }
}
