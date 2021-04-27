// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System.IO;

namespace MDSDK.Dicom.PixelData.RLE
{
    internal class Header
    {
        public uint NumberOfSegments { get; set; }

        public uint[] SegmentOffsets { get; set; } = new uint[15];

        public static Header ReadFrom(BufferedStreamReader input)
        {
            var dataReader = new BinaryDataReader(input, ByteOrder.LittleEndian);

            var header = new Header
            {
                NumberOfSegments = dataReader.Read<uint>()
            };

            if (header.NumberOfSegments > 15)
            {
                throw new IOException($"NumberOfSegments {header.NumberOfSegments} in RLE header out of range");
            }

            for (var i = 0; i < header.NumberOfSegments; i++)
            {
                header.SegmentOffsets[i] = dataReader.Read<uint>();
            }

            dataReader.Input.SkipBytes(4 * (15 - header.NumberOfSegments));

            return header;
        }
    }
}
