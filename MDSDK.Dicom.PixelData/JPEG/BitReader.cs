// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System.IO;
using System.Runtime.CompilerServices;

namespace MDSDK.Dicom.PixelData.JPEG
{
    class BitReader
    {
        private readonly BinaryStreamReader _input;

        private byte _bits;

        private int _bitCount;

        internal BitReader(BinaryStreamReader input)
        {
            _input = input;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int ReadBit()
        {
            if (_bitCount == 0)
            {
                _bits = _input.ReadByte();
                _bitCount = 8;

                if (_bits == 0xFF)
                {
                    var stuffByte = _input.ReadByte();
                    if (stuffByte != 0)
                    {
                        throw new IOException($"Expected zero stuff byte after 0xFF but got 0x{stuffByte:X2}");
                    }
                }
            }

            return (_bits >> --_bitCount) & 1;
        }
    }
}
