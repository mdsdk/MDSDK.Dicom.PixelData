// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System.IO;

namespace MDSDK.Dicom.PixelData.RLE
{
    internal class RLEFormatReader
    {
        private readonly BufferedStreamReader _input;

        private readonly long _startPosition;

        private readonly Header _header;

        public RLEFormatReader(BufferedStreamReader input)
        {
            _input = input;

            _startPosition = input.Position;

            _header = Header.ReadFrom(_input);
        }

        public uint NumberOfSegments
        {
            get { return _header.NumberOfSegments; }
        }

        private int _segmentIndex;

        public void DecodeNextSegment(byte[] buffer, int offset, int count)
        {
            var relativeSegmentPosition = _header.SegmentOffsets[_segmentIndex++];

            if (_input.Position != _startPosition + relativeSegmentPosition)
            {
                throw new IOException($"Current stream position {_input.Position} does not equal sum of "
                    + $"start position {_startPosition} and segment offset {relativeSegmentPosition}");
            }

            var i = offset;
            var end = offset + count;

            while (i < end)
            {
                var n = (sbyte)_input.ReadByte();
                if (n == -128)
                {
                    continue;
                }
                else if (n >= 0)
                {
                    for (var j = 0; j < n + 1; j++)
                    {
                        buffer[i++] = _input.ReadByte();
                    }
                }
                else
                {
                    var b = _input.ReadByte();
                    for (var j = 0; j < -n + 1; j++)
                    {
                        buffer[i++] = b;
                    }
                }
            }
        }
    }
}
