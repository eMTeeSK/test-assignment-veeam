using System;

namespace GZipTest.Core
{
    [Flags]
    public enum GzipFlag
    {
        FTEXT = 1,
        FHCRC = 2,
        FEXTRA = 4,
        FNAME = 8,
        FCOMMENT = 16
    }
}
