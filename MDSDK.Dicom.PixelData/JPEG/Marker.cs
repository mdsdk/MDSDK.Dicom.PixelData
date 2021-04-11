// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Diagnostics;

namespace MDSDK.Dicom.PixelData.JPEG
{
    // From ISO/IEC 10918-1 : 1993(E), Table B.1 – Marker code assignments

    internal static class Marker
    {
        // Start Of Frame markers, non-differential, Huffman coding

        public const ushort SOF0 = 0xFFC0; //  0000 - Baseline DCT 
        public const ushort SOF1 = 0xFFC1; //  0001 - Extended sequential DCT 
        public const ushort SOF2 = 0xFFC2; //  0010 - Progressive DCT 
        public const ushort SOF3 = 0xFFC3; //  0011 - Lossless (sequential)

        // Start Of Frame markers, differential, Huffman coding

        public const ushort SOF5 = 0xFFC5; //  0101 - Differential sequential DCT 
        public const ushort SOF6 = 0xFFC6; //  0110 - Differential progressive DCT  
        public const ushort SOF7 = 0xFFC7; //  0111 - Differential lossless (sequential)

        // Start Of Frame markers, non-differential, arithmetic coding

        public const ushort JPG1 = 0xFFC8; //  1000 - Reserved for JPEG extensions 
        public const ushort SOF9 = 0xFFC9; //  1001 - Extended sequential DCT 
        public const ushort SOF10 = 0xFFCA; // 1010 - Progressive DCT
        public const ushort SOF11 = 0xFFCB; // 1011 - Lossless (sequential)

        // Start Of Frame markers, differential, arithmetic coding

        public const ushort SOF13 = 0xFFCD; // 1101 - Differential sequential DCT 
        public const ushort SOF14 = 0xFFCE; // 1110 - Differential progressive DCT 
        public const ushort SOF15 = 0xFFCF; // 1111 - Differential lossless (sequential)

        public static bool IsStartOfFrameMarker(ushort marker)
        {
            return (marker & 0xFFC0) == 0xFFC0;
        }

        public static CodingType GetFrameCodingType(ushort startOfFrameMarker)
        {
            Debug.Assert(IsStartOfFrameMarker(startOfFrameMarker));
            return ((startOfFrameMarker & 8) == 0) ? CodingType.Huffman : CodingType.Arithmetic;
        }

        public static FrameRelationType GetFrameRelationType(ushort startOfFrameMarker)
        {
            Debug.Assert(IsStartOfFrameMarker(startOfFrameMarker));
            return ((startOfFrameMarker & 4) == 0) ? FrameRelationType.NonDifferential : FrameRelationType.Differential;
        }

        public static CompressionMethod GetCompressionMethod(ushort startOfFrameMarker)
        {
            Debug.Assert(IsStartOfFrameMarker(startOfFrameMarker));
            return ((startOfFrameMarker & 3) == 3) ? CompressionMethod.Lossless : CompressionMethod.NonLossless;
        }

        // Huffman table specification

        public const ushort DHT = 0xFFC4; // Define Huffman table(s)

        // Arithmetic coding conditioning specification

        public const ushort DAC = 0xFFCC; // Define arithmetic coding conditioning(s)

        public struct Range
        {
            public readonly ushort Min;
            public readonly ushort Max;

            public Range(ushort min, ushort max)
            {
                Min = min;
                Max = max;
            }

            public bool Contains(ushort marker)
            {
                return (marker >= Min) && (marker <= Max);
            }
        }

        // Restart interval termination

        public static readonly Range RST = new Range(0xFFD0, 0xFFD7); // Restart with modulo 8 count m

        // Other markers

        public const ushort SOI = 0xFFD8; // Start of image 
        public const ushort EOI = 0xFFD9; // End of image 
        public const ushort SOS = 0xFFDA; // Start of scan 
        public const ushort DQT = 0xFFDB; // Define quantization table(s) 
        public const ushort DNL = 0xFFDC; // Define number of lines
        public const ushort DRI = 0xFFDD; // Define restart interval 
        public const ushort DHP = 0xFFDE; // Define hierarchical progression 
        public const ushort EXP = 0xFFDF; // Expand reference component(s)
        public static readonly Range APP = new Range(0xFFE0, 0xFFEF); // Reserved for application segments  
        public static readonly Range JPG2 = new Range(0xFFF0, 0xFFFD); // Reserved for JPEG extensions 
        public const ushort COM = 0xFFFE; //  Comment

        // Reserved markers

        public const ushort TEM = 0xFF01; // For temporary private use in arithmetic coding 
        public static readonly Range RES = new Range(0xFF02, 0xFFBF); // Reserved
    }
}
