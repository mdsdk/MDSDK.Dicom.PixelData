// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using MDSDK.Dicom.PixelData.JPEG;
using System;

namespace MDSDK.Dicom.PixelData.PixelDataDecoders
{
    internal class JPEGLosslessNonDifferentialHuffman1PixelDataDecoder : EncapsulatedPixelDataDecoder
    {
        public static JPEGLosslessNonDifferentialHuffman1PixelDataDecoder Instance { get; } = new JPEGLosslessNonDifferentialHuffman1PixelDataDecoder();

        private static int[] TryDecodeImage(BufferedStreamReader input, DicomImagePixelDescription desc, out string whyNot)
        {
            var decoder = new JPEGInterchangeFormatReader(input);

            if (decoder.CompressionMethod != CompressionMethod.Lossless)
            {
                whyNot = $"Incompatible compression method {decoder.CompressionMethod} in input";
            }
            else if (decoder.FrameRelationType != FrameRelationType.NonDifferential)
            {
                whyNot = $"Incompatible frame relation type {decoder.FrameRelationType} in input";
            }
            else if (decoder.CodingType != CodingType.Huffman)
            {
                whyNot = $"Incompatible coding type {decoder.CodingType} in input";
            }
            else if (decoder.PredictorSelection != 1)
            {
                whyNot = $"Incompatible predictor selection {decoder.PredictorSelection} in input";
            }
            else if (decoder.NumberOfLines != desc.Rows)
            {
                whyNot = $"Number of lines {decoder.NumberOfLines} in input does not match " +
                    $"Rows value {desc.Rows} in DICOM header";
            }
            else if (decoder.NumberOSamplesPerLine != desc.Columns)
            {
                whyNot = $"Number of samples per line {decoder.NumberOSamplesPerLine} in input does not match " +
                    $"Columns value {desc.Columns} in DICOM header";
            }
            else if (decoder.NumberOfImageComponents != desc.SamplesPerPixel)
            {
                whyNot = $"Number of image components {decoder.NumberOfImageComponents} in input does not match " +
                    $"SamplesPerPixel value {desc.SamplesPerPixel} in DICOM header";
            }
            else
            {
                var image = new int[desc.Rows * desc.Columns * desc.SamplesPerPixel];
                decoder.DecodePixelData(image, 0, image.Length);
                whyNot = null;
                return image;
            }
            return null;
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