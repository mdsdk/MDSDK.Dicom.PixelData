// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization;
using System;
using System.Diagnostics;
using static MDSDK.Dicom.PixelData.StaticInclude;

namespace MDSDK.Dicom.PixelData.PixelDataDecoders
{
    internal sealed class UncompressedPixelDataDecoder : DicomPixelDataDecoder
    {
        public static UncompressedPixelDataDecoder Instance { get; } = new UncompressedPixelDataDecoder();

        public override long[] GetPixelDataFramePositions(DicomStreamReader dicomStreamReader, DicomImagePixelDescription desc,
            int numberOfFrames)
        {
            if (!dicomStreamReader.SkipToPixelData(out uint valueLength))
            {
                throw new Exception($"Missing PixelData");
            }
           
            var frameSize = desc.GetFrameSizeInBytes();

            if ((valueLength != uint.MaxValue) && (valueLength != numberOfFrames * frameSize))
            {
                throw new Exception($"PixelData value length {valueLength} != {numberOfFrames} * {frameSize}");
            }

            var pixelDataFramePositions = new long[numberOfFrames];
            for (var i = 0; i < numberOfFrames; i++)
            {
                var pixelDataFramePosition = dicomStreamReader.Input.Position + i * frameSize;
                var endOfPixelDataFrame = pixelDataFramePosition + frameSize;
                if (endOfPixelDataFrame > dicomStreamReader.Input.Length)
                {
                    throw new Exception($"End of pixel data frame {i} ({pixelDataFramePosition} + {frameSize}) "
                        + $" > length of input stream ({dicomStreamReader.Input.Length})");
                }
                pixelDataFramePositions[i] = pixelDataFramePosition;
            }
            return pixelDataFramePositions;
        }

        public override void DecodePixelDataFrame(DicomStreamReader dicomStreamReader, DicomImagePixelDescription desc, Memory<byte> outputBuffer)
        {
            var input = dicomStreamReader.Input;

            if (input.ByteOrder != ByteOrder.LittleEndian)
            {
                throw NotSupported(nameof(input.ByteOrder), input.ByteOrder);
            }
            
            var frameSize = desc.GetFrameSizeInBytes();
            if (outputBuffer.Length < frameSize)
            {
                throw new ArgumentException(nameof(outputBuffer), "Output buffer too small");
            }

            if (BinaryIOUtils.NativeByteOrder == ByteOrder.LittleEndian)
            {
                input.ReadAll(outputBuffer.Span[0..frameSize]);
            }
            else
            {
                throw NotSupported(nameof(BinaryIOUtils.NativeByteOrder), BinaryIOUtils.NativeByteOrder);
            }
        }
    }
}
