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

namespace Titanium.Web.Proxy.Http2.Hpack
{
    public class HuffmanDecoder
    {
        /// <summary>
        /// Huffman Decoder
        /// </summary>
        public static readonly HuffmanDecoder Instance = new HuffmanDecoder();

        private readonly Node root;

        /// <summary>
        /// Creates a new Huffman decoder with the specified Huffman coding.
        /// </summary>
        private HuffmanDecoder()
        {
            // the Huffman codes indexed by symbol
            var codes = HpackUtil.HuffmanCodes;

            // the length of each Huffman code
            var lengths = HpackUtil.HuffmanCodeLengths;
            if (codes.Length != 257 || codes.Length != lengths.Length)
            {
                throw new ArgumentException("invalid Huffman coding");
            }

            root = BuildTree(codes, lengths);
        }

        /// <summary>
        /// Decompresses the given Huffman coded string literal.
        /// </summary>
        /// <param name="buf">the string literal to be decoded</param>
        /// <returns>the output stream for the compressed data</returns>
        /// <exception cref="IOException">throws IOException if an I/O error occurs. In particular, an <code>IOException</code> may be thrown if the output stream has been closed.</exception>
        public ReadOnlyMemory<byte> Decode(byte[] buf)
        {
            var resultBuf = new byte[buf.Length * 2];
            int resultSize = 0;
            var node = root;
            int current = 0;
            int bits = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                int b = buf[i];
                current = (current << 8) | b;
                bits += 8;
                while (bits >= 8)
                {
                    int c = (current >> (bits - 8)) & 0xFF;
                    node = node.Children![c];
                    bits -= node.Bits;
                    if (node.IsTerminal)
                    {
                        if (node.Symbol == HpackUtil.HuffmanEos)
                        {
                            throw new IOException("EOS Decoded");
                        }

                        resultBuf[resultSize++] = (byte)node.Symbol;
                        node = root;
                    }
                }
            }

            while (bits > 0)
            {
                int c = (current << (8 - bits)) & 0xFF;
                node = node.Children![c];
                if (node.IsTerminal && node.Bits <= bits)
                {
                    bits -= node.Bits;
                    resultBuf[resultSize++] = (byte)node.Symbol;
                    node = root;
                }
                else
                {
                    break;
                }
            }

            // Section 5.2. String Literal Representation
            // Padding not corresponding to the most significant bits of the code
            // for the EOS symbol (0xFF) MUST be treated as a decoding error.
            int mask = (1 << bits) - 1;
            if ((current & mask) != mask)
            {
                throw new IOException("Invalid Padding");
            }

            return resultBuf.AsMemory(0, resultSize);
        }

        private class Node
        {
            // terminal nodes have a symbol
            public int Symbol { get; }

            // number of bits matched by the node
            public int Bits { get; }

            // internal nodes have children
            public Node[]? Children { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="HuffmanDecoder"/> class.
            /// </summary>
            public Node()
            {
                Symbol = 0;
                Bits = 8;
                Children = new Node[256];
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="HuffmanDecoder"/> class.
            /// </summary>
            /// <param name="symbol">the symbol the node represents</param>
            /// <param name="bits">the number of bits matched by this node</param>
            public Node(int symbol, int bits)
            {
                //assert(bits > 0 && bits <= 8);
                Symbol = symbol;
                Bits = bits;
                Children = null;
            }

            public bool IsTerminal => Children == null;
        }

        private static Node BuildTree(int[] codes, byte[] lengths)
        {
            var root = new Node();
            for (int i = 0; i < codes.Length; i++)
            {
                Insert(root, i, codes[i], lengths[i]);
            }

            return root;
        }

        private static void Insert(Node root, int symbol, int code, byte length)
        {
            // traverse tree using the most significant bytes of code
            var current = root;
            while (length > 8)
            {
                if (current.IsTerminal)
                {
                    throw new InvalidDataException("invalid Huffman code: prefix not unique");
                }

                length -= 8;
                int i = (code >> length) & 0xFF;
                if (current.Children![i] == null)
                {
                    current.Children[i] = new Node();
                }

                current = current.Children[i];
            }

            var terminal = new Node(symbol, length);
            int shift = 8 - length;
            int start = (code << shift) & 0xFF;
            int end = 1 << shift;
            for (int i = start; i < start + end; i++)
            {
                current.Children![i] = terminal;
            }
        }
    }
}
