// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.PixelData.RLE;
using System;
using System.Diagnostics;

namespace MDSDK.Dicom.PixelData.PixelDataDecoders
{
    internal class RunLengthPixelDataDecoder : EncapsulatedPixelDataDecoder
    {
        public static RunLengthPixelDataDecoder Instance { get; } = new RunLengthPixelDataDecoder();

        private int[] TryDecodeImage(BufferedStreamReader input, DicomImagePixelDescription desc, out string whyNot)
        {
            if ((desc.BitsAllocated % 8) != 0)
            {
                whyNot = $"Unsupported BitsAllocated {desc.BitsAllocated} in DICOM header";
                return null;
            }

            var bytesPerSample = desc.BitsAllocated / 8;

            var decoder = new RLEFormatReader(input);

            if (decoder.NumberOfSegments != desc.SamplesPerPixel * bytesPerSample)
            {
                whyNot = $"Number of segments {decoder.NumberOfSegments} in run-length encoded input " +
                    $"does not match SamplesPerPixel {desc.SamplesPerPixel} " +
                    $"and BitsAllocated {desc.BitsAllocated} values in DICOM header";
                return null;
            }
            
            var image = new int[desc.Rows * desc.Columns * desc.SamplesPerPixel];

            var segments = new byte[decoder.NumberOfSegments][];

            var segmentLength = desc.Rows * desc.Columns;

            for (var i = 0; i < decoder.NumberOfSegments; i++)
            {
                var segment = new byte[segmentLength];
                decoder.DecodeNextSegment(segment, 0, segment.Length);
                segments[i] = segment;
            }

            if ((bytesPerSample == 1) && (desc.SamplesPerPixel == 1))
            {
                Debug.Assert(segments.Length == 1);
                var segment = segments[0];
                for (var i = 0; i < segmentLength; i++)
                {
                    image[i] = segment[i];
                }
            }
            else if ((bytesPerSample == 1) && (desc.SamplesPerPixel == 3))
            {
                Debug.Assert(segments.Length == 3);
                var redSegment = segments[0];
                var greenSegment = segments[1];
                var blueSegment = segments[2];
                var j = 0;
                for (var i = 0; i < segmentLength; i++)
                {
                    image[j++] = redSegment[i];
                    image[j++] = greenSegment[i];
                    image[j++] = blueSegment[i];
                }
            }
            else if ((bytesPerSample == 2) && (desc.SamplesPerPixel == 1))
            {
                Debug.Assert(segments.Length == 2);
                var msbSegment = segments[0];
                var lsbSegment = segments[1];
                for (var i = 0; i < segmentLength; i++)
                {
                    image[i] = (msbSegment[i] << 8) | lsbSegment[i];
                }
            }
            else
            {
                whyNot = "Unsupported pixel format";
                return null;
            }

            whyNot = null;
            return image;
        }

        protected override void DecodePixelDataFrame(BufferedStreamReader input, DicomImagePixelDescription desc, Memory<byte> output)
        {
            var image = TryDecodeImage(input, desc, out string whyNot);
            if (image == null)
            {
                throw new Exception("Cannot decode image: " + whyNot);
            }
            CopyImageToPixelDataBuffer(desc, image, output);
        }
    }
}

