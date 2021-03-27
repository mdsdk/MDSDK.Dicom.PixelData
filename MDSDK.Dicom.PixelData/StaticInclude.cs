using System;

namespace MDSDK.Dicom.PixelData
{
    internal static class StaticInclude
    {
        public static NotSupportedException NotSupported<T>(string name, T value)
        {
            return new NotSupportedException($"Unsupported {name}: {value}");
        }
    }
}
