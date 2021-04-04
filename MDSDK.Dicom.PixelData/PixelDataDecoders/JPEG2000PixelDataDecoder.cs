// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.JPEG2000;
using System;
using System.Runtime.InteropServices;
using static MDSDK.Dicom.PixelData.StaticInclude;

namespace MDSDK.Dicom.PixelData.PixelDataDecoders
{
    class JPEG2000PixelDataDecoder : EncapsulatedPixelDataDecoder
    {
        public static JPEG2000PixelDataDecoder Instance { get; } = new JPEG2000PixelDataDecoder();

        protected override void DecodePixelDataFrame(BinaryStreamReader input, DicomImagePixelDescription desc, Memory<byte> output)
        {
            var image = JPEG2000Decoder.DecodeImage(input);
            
            if (desc.SamplesPerPixel == 1)
            {
                if (desc.BitsAllocated == 16)
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
