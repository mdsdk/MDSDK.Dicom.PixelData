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

        public abstract void DecodePixelDataFrame(DicomStreamReader dicomStreamReader, DicomImagePixelDescription desc, Memory<byte> output);

        private static readonly Dictionary<DicomUID, DicomPixelDataDecoder> TransferSyntaxPixelDataDecoders = new()
        {
            { DicomUID.TransferSyntax.ImplicitVRLittleEndian, UncompressedPixelDataDecoder.Instance },
            { DicomUID.TransferSyntax.ExplicitVRLittleEndian, UncompressedPixelDataDecoder.Instance },
            { DicomUID.TransferSyntax.JPEGLossless, JPEGLosslessNonDifferentialHuffman1PixelDataDecoder.Instance },
            { DicomUID.TransferSyntax.JPEG2000, JPEG2000PixelDataDecoder.Instance },
            { DicomUID.TransferSyntax.JPEG2000Lossless, JPEG2000PixelDataDecoder.Instance },
            { DicomUID.TransferSyntax.RLELossless, RunLengthPixelDataDecoder.Instance },
        };

        public static bool TryGet(DicomUID transferSyntaxUID, out DicomPixelDataDecoder pixelDataDecoder)
        {
            return TransferSyntaxPixelDataDecoders.TryGetValue(transferSyntaxUID, out pixelDataDecoder);
        }
    }
}
