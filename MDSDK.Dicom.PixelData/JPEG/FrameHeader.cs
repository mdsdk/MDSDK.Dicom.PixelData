// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System.IO;

namespace MDSDK.Dicom.PixelData.JPEG
{
    internal class FrameHeader
    {
        public ushort Lf_FrameHeaderLength { get; set; }

        public byte P_SamplePrecision { get; set; }

        public ushort Y_NumberOfLines { get; set; }

        public ushort X_NumberOSamplesPerLine { get; set; }

        public ushort Nf_NumberOfImageComponentsInFrame { get; set; }

        public class ComponentSpecification
        {
            public byte C_ComponentIdentifier { get; set; }

            public byte H_V_SamplingFactors { get; set; }

            public byte Tq_QuantizationTableDestinationSelector { get; set; }

            public static ComponentSpecification ReadFrom(BinaryStreamReader input)
            {
                return new ComponentSpecification
                {
                    C_ComponentIdentifier = input.ReadByte(),
                    H_V_SamplingFactors = input.ReadByte(),
                    Tq_QuantizationTableDestinationSelector = input.ReadByte()
                };
            }
        }

        public ComponentSpecification[] ComponentSpecifications { get; set; }

        public static FrameHeader ReadFrom(BinaryStreamReader input)
        {
            var frameHeader = new FrameHeader
            {
                Lf_FrameHeaderLength = input.Read<ushort>(),
                P_SamplePrecision = input.ReadByte(),
                Y_NumberOfLines = input.Read<ushort>(),
                X_NumberOSamplesPerLine = input.Read<ushort>(),
                Nf_NumberOfImageComponentsInFrame = input.ReadByte()
            };

            var Lf = frameHeader.Lf_FrameHeaderLength;
            var Nf = frameHeader.Nf_NumberOfImageComponentsInFrame;

            if (Lf != 8 + 3 * Nf)
            {
                throw new IOException($"Lf {Lf} != 8 + 3 x Nf {Nf}");
            }

            var componentSpecifications = new ComponentSpecification[frameHeader.Nf_NumberOfImageComponentsInFrame];
            for (var i = 0; i < componentSpecifications.Length; i++)
            {
                componentSpecifications[i] = ComponentSpecification.ReadFrom(input);
            }
            frameHeader.ComponentSpecifications = componentSpecifications;

            return frameHeader;
        }
    }
}
