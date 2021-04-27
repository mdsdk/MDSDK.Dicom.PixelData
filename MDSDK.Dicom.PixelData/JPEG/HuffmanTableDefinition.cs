// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.BinaryIO;
using System.Linq;

namespace MDSDK.Dicom.PixelData.JPEG
{
    internal class HuffmanTableDefinition
    {
        public byte Tc_Th_TableClass_and_HuffmanTableIdentifier { get; set; }

        public byte[] L_NumberOfHuffmanCodesOfLengths1to16 { get; set; }

        public byte[] V_ValueAssociatedWithEachHuffmanCode { get; set; }

        private static HuffmanTableDefinition ReadFrom(BufferedStreamReader input, ref int remainingLength)
        {
            var huffmanTableDefinition = new HuffmanTableDefinition
            {
                Tc_Th_TableClass_and_HuffmanTableIdentifier = input.ReadByte(),
                L_NumberOfHuffmanCodesOfLengths1to16 = input.ReadBytes(16)
            };

            var numberOfHuffmanCodes = huffmanTableDefinition.L_NumberOfHuffmanCodesOfLengths1to16.Sum(b => b);
            huffmanTableDefinition.V_ValueAssociatedWithEachHuffmanCode = input.ReadBytes(numberOfHuffmanCodes);

            remainingLength -= (17 + numberOfHuffmanCodes);

            return huffmanTableDefinition;
        }

        public static HuffmanTableDefinition[] ReadArrayFrom(BinaryDataReader dataReader)
        {
            var Lh_HuffmanTableDefinitionLength = dataReader.Read<ushort>();

            var remainingLength = Lh_HuffmanTableDefinitionLength - 2;

            var huffmanTableDefinitions = new HuffmanTableDefinition[4];
            for (var i = 0; (i < huffmanTableDefinitions.Length) && (remainingLength > 0); i++)
            {
                huffmanTableDefinitions[i] = HuffmanTableDefinition.ReadFrom(dataReader.Input, ref remainingLength);
            }
            return huffmanTableDefinitions;
        }
    }
}

