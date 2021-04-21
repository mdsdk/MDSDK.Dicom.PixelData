// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System;
using System.IO;

namespace MDSDK.Dicom.PixelData.JPEG
{
    internal class JPEGInterchangeFormatReader
    {
        private readonly BinaryStreamReader _input;

        private readonly ushort _startOfFrameMarker;

        private readonly FrameHeader _frameHeader;

        private readonly ScanHeader _scanHeader;

        private readonly HuffmanTable[] _huffmanTables = new HuffmanTable[4];

        public JPEGInterchangeFormatReader(BinaryStreamReader input)
        {
            _input = input;

            var marker = ReadMarker();
            if (marker != Marker.SOI)
            {
                throw new IOException($"Expected SOI but got {marker:X4}");
            }

            do
            {
                marker = ReadMarker();
            }
            while (IfIsMiscellanenousMarkerSegmentThenProcessIt(marker));

            if (!Marker.IsStartOfFrameMarker(marker))
            {
                throw new IOException($"Expected SOF marker but got {marker:X4}");
            }

            _startOfFrameMarker = marker;

            _frameHeader = FrameHeader.ReadFrom(_input);

            do
            {
                marker = ReadMarker();
            }
            while (IfIsMiscellanenousMarkerSegmentThenProcessIt(marker));

            if (marker != Marker.SOS)
            {
                throw new IOException($"Expected SOS marker but got {marker:X4}");
            }

            _scanHeader = ScanHeader.ReadFrom(_input);
        }

        public CodingType CodingType
        {
            get { return Marker.GetFrameCodingType(_startOfFrameMarker); }
        }

        public FrameRelationType FrameRelationType
        {
            get { return Marker.GetFrameRelationType(_startOfFrameMarker); }
        }

        public CompressionMethod CompressionMethod
        {
            get { return Marker.GetCompressionMethod(_startOfFrameMarker); }
        }

        public int SamplePrecision
        {
            get { return _frameHeader.P_SamplePrecision; }
        }

        public int NumberOfLines
        {
            get { return _frameHeader.Y_NumberOfLines; }
        }

        public int NumberOSamplesPerLine
        {
            get { return _frameHeader.X_NumberOSamplesPerLine; }
        }

        public int NumberOfImageComponents
        {
            get { return _frameHeader.ComponentSpecifications.Length; }
        }

        public int PredictorSelection
        {
            get { return _scanHeader.Ss_StartOfSpectral_or_PredictorSelection; }
        }

        private ushort ReadMarker()
        {
            var b = _input.ReadByte();

            if (b != 0xFF)
            {
                throw new IOException($"Expected 0xFF to start marker but got 0x{b:X2}");
            }

            do
            {
                b = _input.ReadByte();
            }
            while (b == 0xFF);

            return (ushort)(0xFF00 | b);
        }

        private void ProcessDefineHuffmanTableSegment(ushort marker)
        {
            var huffmanTableDefinitions = HuffmanTableDefinition.ReadArrayFrom(_input);
            foreach (var huffmanTableDefinition in huffmanTableDefinitions)
            {
                if (huffmanTableDefinition == null)
                {
                    break;
                }
                var tableClass = huffmanTableDefinition.Tc_Th_TableClass_and_HuffmanTableIdentifier >> 4;
                if (tableClass != 0)
                {
                    throw new NotSupportedException($"Unsupported table class {tableClass}");
                }
                var huffmanTableIdentifer = huffmanTableDefinition.Tc_Th_TableClass_and_HuffmanTableIdentifier & 0xF;
                _huffmanTables[huffmanTableIdentifer] = new HuffmanTable(huffmanTableDefinition);
            }
        }

        private void SkipIgnorableMiscellanenousMarkerSegment()
        {
            var length = _input.Read<ushort>();
            _input.SkipBytes(length - 2); // length includes the two bytes of the length field itself
        }

        private bool IfIsMiscellanenousMarkerSegmentThenProcessIt(ushort marker)
        {
            if (marker == Marker.DQT)
            {
                throw new NotSupportedException("Use of quantization tables is not supported");
            }
            else if (marker == Marker.DHT)
            {
                ProcessDefineHuffmanTableSegment(marker);
                return true;
            }
            else if (marker == Marker.DAC)
            {
                throw new NotSupportedException("Use of arithmetic conditioning tables is not supported");
            }
            else if (marker == Marker.DRI)
            {
                throw new NotSupportedException("Use of restart intervals is not supported");
            }
            else if ((marker == Marker.COM) || Marker.APP.Contains(marker))
            {
                SkipIgnorableMiscellanenousMarkerSegment();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DecodePixelData(int[] buffer, int offset, int count)
        {
            if (CodingType != CodingType.Huffman)
            {
                throw new NotSupportedException($"Unsupported CodingType {CodingType}");
            }

            if (FrameRelationType != FrameRelationType.NonDifferential)
            {
                throw new NotSupportedException($"Unsupported FrameRelationType {FrameRelationType}");
            }

            if (CompressionMethod != CompressionMethod.Lossless)
            {
                throw new NotSupportedException($"Unsupported CompressionMethod {CompressionMethod}");
            }

            if ((SamplePrecision < 2) || (SamplePrecision > 16))
            {
                throw new NotSupportedException($"Unsupported SamplePrecision {SamplePrecision}");
            }

            var hvSamplingFactors = _frameHeader.ComponentSpecifications[0].H_V_SamplingFactors;
            if (hvSamplingFactors != 0x11)
            {
                throw new IOException($"Unexpected H_V_SamplingFactors {hvSamplingFactors:X2} for single component scan");
            }

            if (NumberOfImageComponents == 1)
            {
                DecodeSingleComponentHuffmanCodedScan(buffer, offset, count);
            }
            else if (NumberOfImageComponents == 3)
            {
                DecodeTripleComponentHuffmanCodedScan(buffer, offset, count);
            }
            else
            {
                throw new NotSupportedException($"Unsupported NumberOfImageComponents {NumberOfImageComponents}");
            }

            var marker = ReadMarker();

            if (marker != Marker.EOI)
            {
                throw new IOException($"Unexpected marker {marker:X4} after scan");
            }
        }

        private HuffmanTable GetHuffmanTable(int componentIndex)
        {
            var scanComponent = _scanHeader.ComponentSpecifications[componentIndex];
            var huffmanTableIndex = scanComponent.Td_Ta_EntropyCodingTableDestinationSelectors >> 4;
            return _huffmanTables[huffmanTableIndex];
        }

        private void DecodeSingleComponentHuffmanCodedScan(int[] buffer, int offset, int count)
        {
            if (PredictorSelection != 1)
            {
                throw new NotSupportedException($"Unsupported PredictorSelection {PredictorSelection}");
            }

            var rows = _frameHeader.Y_NumberOfLines;
            var columns = _frameHeader.X_NumberOSamplesPerLine;

            if (count != rows * columns)
            {
                throw new ArgumentException($"count {count} != NumberOfLines {rows} * NumberOfSamplesPerLine {columns}");
            }

            var P = _frameHeader.P_SamplePrecision;
            var Pt = _scanHeader.Ah_Al_SuccessiveApproximationBitPositions_or_PointTransform & 0x0F;

            var startOfImagePredictionValue = 1 << (P - Pt - 1);
            var startOfLinePredictionValue = startOfImagePredictionValue;

            var huffmanTable = GetHuffmanTable(0);

            var bitStreamReader = new BitReader(_input);

            for (var row = 0; row < rows; row++)
            {
                startOfLinePredictionValue += DecodeDifference(huffmanTable, bitStreamReader);

                var pixelValue = startOfLinePredictionValue;
                buffer[offset++] = pixelValue;

                for (var column = 1; column < columns; column++)
                {
                    pixelValue += DecodeDifference(huffmanTable, bitStreamReader);
                    buffer[offset++] = pixelValue;
                }
            }
        }

        private void DecodeTripleComponentHuffmanCodedScan(int[] buffer, int offset, int count)
        {
            if (PredictorSelection != 1)
            {
                throw new NotSupportedException($"Unsupported PredictorSelection {PredictorSelection}");
            }

            var rows = _frameHeader.Y_NumberOfLines;
            var columns = _frameHeader.X_NumberOSamplesPerLine;

            if (count != 3 * rows * columns)
            {
                throw new ArgumentException($"count {count} != 3 * NumberOfLines {rows} * NumberOfSamplesPerLine {columns}");
            }

            var P = _frameHeader.P_SamplePrecision;
            var Pt = _scanHeader.Ah_Al_SuccessiveApproximationBitPositions_or_PointTransform & 0x0F;

            var startOfImagePredictionValue = 1 << (P - Pt - 1);

            var startOfLinePredictionValue0 = startOfImagePredictionValue;
            var startOfLinePredictionValue1 = startOfImagePredictionValue;
            var startOfLinePredictionValue2 = startOfImagePredictionValue;

            var huffmanTable0 = GetHuffmanTable(0);
            var huffmanTable1 = GetHuffmanTable(1);
            var huffmanTable2 = GetHuffmanTable(2);

            var bitStreamReader = new BitReader(_input);

            for (var row = 0; row < rows; row++)
            {
                startOfLinePredictionValue0 += DecodeDifference(huffmanTable0, bitStreamReader);
                startOfLinePredictionValue1 += DecodeDifference(huffmanTable1, bitStreamReader);
                startOfLinePredictionValue2 += DecodeDifference(huffmanTable2, bitStreamReader);

                var pixelValue0 = startOfLinePredictionValue0;
                var pixelValue1 = startOfLinePredictionValue1;
                var pixelValue2 = startOfLinePredictionValue2;

                buffer[offset++] = pixelValue0;
                buffer[offset++] = pixelValue1;
                buffer[offset++] = pixelValue2;

                for (var column = 1; column < columns; column++)
                {
                    pixelValue0 += DecodeDifference(huffmanTable0, bitStreamReader);
                    pixelValue1 += DecodeDifference(huffmanTable1, bitStreamReader);
                    pixelValue2 += DecodeDifference(huffmanTable2, bitStreamReader);

                    buffer[offset++] = pixelValue0;
                    buffer[offset++] = pixelValue1;
                    buffer[offset++] = pixelValue2;
                }
            }
        }

        private int DecodeDifference(HuffmanTable huffmanTable, BitReader input)
        {
            var ssss = huffmanTable.DecodeNextValue(input);

            if (ssss == 0)
            {
                return 0;
            }
            else if (ssss < 16)
            {
                var diff = 0;
                for (var i = 0; i < ssss; i++)
                {
                    diff = (diff << 1) | input.ReadBit();
                }
                if (diff < (1 << (ssss - 1)))
                {
                    diff += (-1 << ssss) + 1;
                }
                return diff;
                /*
                var msb = input.ReadBit();
                var diff = 0;
                for (var i = 1; i < ssss; i++)
                {
                    diff = (diff << 8) | input.ReadBit();
                }
                if (msb == 0)
                {
                    diff -= (1 << ssss) - 1;
                }
                return (short)diff;
                */
            }
            else if (ssss == 16)
            {
                return 1 << 15;
            }
            else
            {
                throw new IOException($"Invalid SSSS value {ssss} decoded");
            }
        }
    }
}

