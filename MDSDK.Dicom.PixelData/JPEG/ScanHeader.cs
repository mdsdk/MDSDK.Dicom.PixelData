// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System.IO;

namespace MDSDK.Dicom.PixelData.JPEG
{
    internal class ScanHeader
    {
        public ushort Ls_ScanHeaderLength { get; set; }

        public byte Ns_NumberOfImageComponentsInScan { get; set; }

        public class ComponentSpecification
        {
            public byte Cs_ScanComponentSelector { get; set; }

            public byte Td_Ta_EntropyCodingTableDestinationSelectors { get; set; }

            public static ComponentSpecification ReadFrom(BinaryDataReader dataReader)
            {
                return new ComponentSpecification
                {
                    Cs_ScanComponentSelector = dataReader.ReadByte(),
                    Td_Ta_EntropyCodingTableDestinationSelectors = dataReader.ReadByte()
                };
            }
        }

        public ComponentSpecification[] ComponentSpecifications { get; set; }

        public byte Ss_StartOfSpectral_or_PredictorSelection { get; set; }

        public byte Se_EndOfSpectralSelection { get; set; }

        public byte Ah_Al_SuccessiveApproximationBitPositions_or_PointTransform  { get; set; }

        public static ScanHeader ReadFrom(BinaryDataReader dataReader)
        {
            var scanHeader = new ScanHeader
            {
                Ls_ScanHeaderLength = dataReader.Read<ushort>(),
                Ns_NumberOfImageComponentsInScan = dataReader.ReadByte()
            };


            var Ls = scanHeader.Ls_ScanHeaderLength;
            var Ns = scanHeader.Ns_NumberOfImageComponentsInScan;

            if (Ls != 6 + 2 * Ns)
            {
                throw new IOException($"Ls {Ls} != 6 + 2 x Ns {Ns}");
            }

            var componentSpecifications = new ComponentSpecification[scanHeader.Ns_NumberOfImageComponentsInScan];
            for (var i = 0; i < componentSpecifications.Length; i++)
            {
                componentSpecifications[i] = ComponentSpecification.ReadFrom(dataReader);
            }
            scanHeader.ComponentSpecifications = componentSpecifications;

            scanHeader.Ss_StartOfSpectral_or_PredictorSelection = dataReader.ReadByte();
            scanHeader.Se_EndOfSpectralSelection = dataReader.ReadByte();
            scanHeader.Ah_Al_SuccessiveApproximationBitPositions_or_PointTransform = dataReader.ReadByte();

            return scanHeader;
        }
    }
}
