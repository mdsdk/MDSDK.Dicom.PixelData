// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace MDSDK.Dicom.PixelData.JPEG
{
    internal class HuffmanTable
    {
        private static string ToBitString(int bits, int length)
        {
            var bitChars = new char[length];
            for (var i = 0; i < bitChars.Length; i++)
            {
                bitChars[bitChars.Length - i - 1] = ((bits & (1 << i)) == 0) ? '0' : '1';
            }
            return new string(bitChars);
        }

        private struct HuffmanCode
        {
            public int Length;
            public int Bits;
            public int Value;

            public HuffmanCode(int length, int bits, int value)
            {
                Length = length;
                Bits = bits;
                Value = value;
            }

            public override string ToString()
            {
                return ToBitString(Bits, Length);
            }
        }

        private readonly HuffmanCode[] _codes;

        public HuffmanTable(HuffmanTableDefinition huffmanTableDefinition)
        {
            var values = huffmanTableDefinition.V_ValueAssociatedWithEachHuffmanCode;

            _codes = new HuffmanCode[values.Length];

            var index = 0;
            var bits = 0;

            for (var length = 1; length <= 16; length++)
            {
                var n = huffmanTableDefinition.L_NumberOfHuffmanCodesOfLengths1to16[length - 1];
                for (var i = 0; i < n; i++)
                {
                    _codes[index] = new HuffmanCode(length, bits, values[index]);
                    index++;
                    bits++;
                }
                bits <<= 1;
            }

            Debug.Assert(index == _codes.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DecodeNextValue(BitReader input)
        {
            var bits = input.ReadBit();
            var length = 1;

            foreach (var code in _codes)
            {
                while (length < code.Length)
                {
                    bits = (bits << 1) | input.ReadBit();
                    length++;
                }
                if ((code.Length == length) && (code.Bits == bits))
                {
                    return code.Value;
                }
            }

            throw new IOException($"Invalid Huffman code {ToBitString(bits, length)} in input");
        }
    }
}
