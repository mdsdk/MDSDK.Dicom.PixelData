// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using static MDSDK.Dicom.PixelData.StaticInclude;

namespace MDSDK.Dicom.PixelData
{
    // Table C.7-11c.Image Pixel Description Macro Attributes

    public class DicomImagePixelDescription
    {
        public ushort SamplesPerPixel { get; set; }

        public string PhotometricInterpretation { get; set; }

        public ushort Rows { get; set; }

        public ushort Columns { get; set; }

        public ushort BitsAllocated { get; set; }

        public ushort BitsStored { get; set; }

        public ushort HighBit { get; set; }

        public DicomPixelRepresentation PixelRepresentation { get; set; }

        public int[] PixelAspectRatio { get; set; }

        public class GrayscaleAttributes
        {
            public int? SmallestImagePixelValue { get; set; }

            public int? LargestImagePixelValue { get; set; }
        }

        public class ColorPaletteAttributes
        {
            public int[] RedPaletteColorLookupTable​Descriptor​ { get; set; }

            public int[] GreenPaletteColorLookupTable​Descriptor​ { get; set; }

            public int[] BluePaletteColorLookupTable​Descriptor​ { get; set; }

            public ushort[] RedPaletteColorLookupTableData { get; set; }

            public ushort[] GreenPaletteColorLookupTableData { get; set; }

            public ushort[] BluePaletteColorLookupTableData { get; set; }
        }

        public class ColorAttributes
        {
            public DicomPlanarConfiguration PlanarConfiguration { get; set; }

            public ColorPaletteAttributes Palette { get; set; }
            
            public byte[] ICCProfile { get; set; }

            public string ColorSpace { get; set; }
        }

        public GrayscaleAttributes Grayscale { get; set; }

        public ColorAttributes Color { get; set; }

        public int GetSampleSizeInBytes()
        {
            if ((BitsAllocated % 8) != 0)
            {
                throw NotSupported(nameof(BitsAllocated), BitsAllocated);
            }
            return (BitsAllocated - 1) / 8 + 1;
        }

        public int GetFramePixelCount() => Rows * Columns;

        public int GetFrameSampleCount() => GetFramePixelCount() * SamplesPerPixel;

        public int GetFrameSizeInBytes() => GetFrameSampleCount() * GetSampleSizeInBytes();
    }
}
