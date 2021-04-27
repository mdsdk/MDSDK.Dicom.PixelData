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

            public static ComponentSpecification ReadFrom(BinaryDataReader dataReader)
            {
                return new ComponentSpecification
                {
                    C_ComponentIdentifier = dataReader.ReadByte(),
                    H_V_SamplingFactors = dataReader.ReadByte(),
                    Tq_QuantizationTableDestinationSelector = dataReader.ReadByte()
                };
            }
        }

        public ComponentSpecification[] ComponentSpecifications { get; set; }

        public static FrameHeader ReadFrom(BinaryDataReader dataReader)
        {
            var frameHeader = new FrameHeader
            {
                Lf_FrameHeaderLength = dataReader.Read<ushort>(),
                P_SamplePrecision = dataReader.ReadByte(),
                Y_NumberOfLines = dataReader.Read<ushort>(),
                X_NumberOSamplesPerLine = dataReader.Read<ushort>(),
                Nf_NumberOfImageComponentsInFrame = dataReader.ReadByte()
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
                componentSpecifications[i] = ComponentSpecification.ReadFrom(dataReader);
            }
            frameHeader.ComponentSpecifications = componentSpecifications;

            return frameHeader;
        }
    }
}
