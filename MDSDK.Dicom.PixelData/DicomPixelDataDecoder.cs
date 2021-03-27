// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.PixelData.PixelDataDecoders;
using MDSDK.Dicom.Serialization;
using System;
using System.Collections.Generic;

namespace MDSDK.Dicom.PixelData
{
    public abstract class DicomPixelDataDecoder
    {
        public abstract long[] GetPixelDataFramePositions(DicomStreamReader dicomStreamReader, DicomImagePixelDescription desc,
            int numberOfFrames);
        
        public abstract void DecodePixelDataFrame(DicomImagePixelDescription desc, BinaryStreamReader input, Span<byte> output);

        private static readonly Dictionary<DicomUID, DicomPixelDataDecoder> TransferSyntaxPixelDataDecoders = new()
        {
            { DicomUID.ImplicitVRLittleEndian, new UncompressedPixelDataDecoder() },
            { DicomUID.ExplicitVRLittleEndian, new UncompressedPixelDataDecoder() }
        };

        public static bool TryGet(DicomUID transferSyntaxUID, out DicomPixelDataDecoder pixelDataDecoder)
        {
            return TransferSyntaxPixelDataDecoders.TryGetValue(transferSyntaxUID, out pixelDataDecoder);
        }
    }
}
