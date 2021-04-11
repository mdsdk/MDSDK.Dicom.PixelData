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
            CopyImageToPixelDataBuffer(desc, image, output);
        }
    }
}
