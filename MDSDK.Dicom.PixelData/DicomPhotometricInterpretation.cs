// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.PixelData
{
    /// <summary>Defined values for the DICOM Photometric Interpretation attribute</summary>
    public static class DicomPhotometricInterpretation
    {
#pragma warning disable 1591

        public const string MONOCHROME1 = "MONOCHROME1";
        public const string MONOCHROME2 = "MONOCHROME2";
        public const string PALETTE_COLOR = "PALETTE COLOR";
        public const string RGB = "RGB";
        public const string YBR_RCT = "YBR_RCT";
        public const string YBR_ICT = "YBR_ICT";

#pragma warning restore 1591

        /// <summary>Returns whether the given Photometric Interpretation string is MONOCHROME1 or MONOCHROME2</summary>
        public static bool IsMonochrome(string photometricInterpretation)
        {
            return (photometricInterpretation == MONOCHROME1)
                || (photometricInterpretation == MONOCHROME2);
        }
    }
}
