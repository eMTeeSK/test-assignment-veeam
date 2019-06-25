using System;

namespace GZipTest.Core
{
    [Flags]
    internal enum GzipFlag
    {
        FTEXT = 1,
        FHCRC = 2,
        FEXTRA = 4,
        FNAME = 8,
        FCOMMENT = 16
    }
}
