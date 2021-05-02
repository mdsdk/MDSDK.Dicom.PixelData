// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

namespace MDSDK.Dicom.PixelData
{
    /// <summary>Defined values for the DICOM Planar Configuration attribute</summary>
    public enum DicomPlanarConfiguration : ushort
    {
#pragma warning disable 1591
        
        ColorByPixel = 0,
        ColorByPlane = 1

#pragma warning restore 1591
    }
}
