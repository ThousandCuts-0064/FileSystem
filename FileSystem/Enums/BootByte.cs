using System;

namespace FileSystemNS
{
    [Flags]
    internal enum BootByte : byte
    {
        InitFail       = 0,
        InitSuccess    = 1 << 0,
        ForcedShutdown = 1 << 1,
        None           = 1 << 2,
        None1          = 1 << 3,
        None2          = 1 << 4,
        None3          = 1 << 5,
        None4          = 1 << 6,
        None5          = 1 << 7,
        All            = byte.MaxValue
    }
}
