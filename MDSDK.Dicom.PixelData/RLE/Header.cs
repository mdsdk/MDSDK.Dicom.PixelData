// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System.IO;

namespace MDSDK.Dicom.PixelData.RLE
{
    internal class Header
    {
        public uint NumberOfSegments { get; set; }

        public uint[] SegmentOffsets { get; set; } = new uint[15];

        public static Header ReadFrom(BinaryStreamReader input)
        {
            var header = new Header
            {
                NumberOfSegments = input.Read<uint>()
            };

            if (header.NumberOfSegments > 15)
            {
                throw new IOException($"NumberOfSegments {header.NumberOfSegments} in RLE header out of range");
            }

            for (var i = 0; i < header.NumberOfSegments; i++)
            {
                header.SegmentOffsets[i] = input.Read<uint>();
            }

            input.SkipBytes(4 * (15 - header.NumberOfSegments));

            return header;
        }
    }
}
