// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization;
using static MDSDK.Dicom.PixelData.StaticInclude;
using System;
using System.Runtime.InteropServices;

namespace MDSDK.Dicom.PixelData.PixelDataDecoders
{
    abstract class EncapsulatedPixelDataDecoder : DicomPixelDataDecoder
    {
        public sealed override long[] GetPixelDataFramePositions(DicomStreamReader dicomStreamReader, DicomImagePixelDescription desc, 
            int numberOfFrames)
        {
            var framePositions = new long[numberOfFrames];
            dicomStreamReader.GetEncapsulatedPixelDataFramePositions(framePositions);
            return framePositions;
        }

        protected abstract void DecodePixelDataFrame(BufferedStreamReader input, DicomImagePixelDescription desc, Memory<byte> output);

        public sealed override void DecodePixelDataFrame(DicomStreamReader dicomStreamReader, DicomImagePixelDescription desc, Memory<byte> output)
        {
            dicomStreamReader.ReadEncapsulatedPixelDataFrame(input => DecodePixelDataFrame(input, desc, output));
        }

        protected void CopyImageToPixelDataBuffer(DicomImagePixelDescription desc, int[] image, Memory<byte> output)
        {
            if (desc.SamplesPerPixel == 1)
            {
                if (desc.BitsAllocated == 8)
                {
                    var span = output.Span;
                    for (var i = 0; i < image.Length; i++)
                    {
                        span[i] = (byte)image[i];
                    }
                }
                else if (desc.BitsAllocated == 16)
                {
                    if (desc.PixelRepresentation == DicomPixelRepresentation.Signed)
                    {
                        var span = MemoryMarshal.Cast<byte, short>(output.Span);
                        for (var i = 0; i < image.Length; i++)
                        {
                            span[i] = (short)image[i];
                        }
                    }
                    else if (desc.PixelRepresentation == DicomPixelRepresentation.Unsigned)
                    {
                        var span = MemoryMarshal.Cast<byte, ushort>(output.Span);
                        for (var i = 0; i < image.Length; i++)
                        {
                            span[i] = (ushort)image[i];
                        }
                    }
                    else
                    {
                        throw NotSupported(nameof(desc.PixelRepresentation), desc.PixelRepresentation);
                    }
                }
                else
                {
                    throw NotSupported(nameof(desc.BitsAllocated), desc.BitsAllocated);
                }
            }
            else if (desc.SamplesPerPixel == 3)
            {
                if (desc.BitsAllocated == 8)
                {
                    var span = output.Span;
                    for (var i = 0; i < image.Length; i++)
                    {
                        span[i] = (byte)image[i];
                    }
                }
                else
                {
                    throw NotSupported(nameof(desc.BitsAllocated), desc.BitsAllocated);
                }
            }
            else
            {
                throw NotSupported(nameof(desc.SamplesPerPixel), desc.SamplesPerPixel);
            }
        }
    }
}
