// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using static MDSDK.Dicom.PixelData.StaticInclude;

namespace MDSDK.Dicom.PixelData
{
    // Table C.7-11c.Image Pixel Description Macro Attributes

    /// <summary>DICOM attributes as defined by the DICOM Image Pixel Description macro</summary>
    public class DicomImagePixelDescription
    {
#pragma warning disable 1591

        public ushort SamplesPerPixel { get; set; }

        public string PhotometricInterpretation { get; set; }

        public ushort Rows { get; set; }

        public ushort Columns { get; set; }

        public ushort BitsAllocated { get; set; }

        public ushort BitsStored { get; set; }

        public ushort HighBit { get; set; }

        public DicomPixelRepresentation PixelRepresentation { get; set; }

        public int[] PixelAspectRatio { get; set; }

#pragma warning restore 1591

        private int GetSampleSizeInBytes()
        {
            if ((BitsAllocated % 8) != 0)
            {
                throw NotSupported(nameof(BitsAllocated), BitsAllocated);
            }
            return (BitsAllocated - 1) / 8 + 1;
        }

        private int GetFramePixelCount() => Rows * Columns;

        private int GetFrameSampleCount() => GetFramePixelCount() * SamplesPerPixel;

        /// <summary>Returns the size in bytes of a single image frame</summary>
        public int GetFrameSizeInBytes() => GetFrameSampleCount() * GetSampleSizeInBytes();
    }
}
