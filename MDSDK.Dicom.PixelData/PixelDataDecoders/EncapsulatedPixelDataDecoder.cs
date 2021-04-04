// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.Serialization;
using System;

namespace MDSDK.Dicom.PixelData.PixelDataDecoders
{
    abstract class EncapsulatedPixelDataDecoder : DicomPixelDataDecoder
    {
        public sealed override long[] GetPixelDataFramePositions(DicomStreamReader dicomStreamReader, DicomImagePixelDescription desc, 
            int numberOfFrames)
        {
            var framePositions = new long[numberOfFrames];
            dicomStreamReader.ReadEncapsulatedPixelDataFramePositions(framePositions);
            return framePositions;
        }

        protected abstract void DecodePixelDataFrame(BinaryStreamReader input, DicomImagePixelDescription desc, Memory<byte> output);

        public sealed override void DecodePixelDataFrame(DicomStreamReader dicomStreamReader, DicomImagePixelDescription desc, Memory<byte> output)
        {
            dicomStreamReader.ReadEncapsulatedPixelDataFrame(input => DecodePixelDataFrame(input, desc, output));
        }
    }
}
