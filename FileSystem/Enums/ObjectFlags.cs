using System;

namespace FileSystemNS
{
    [Flags]
    internal enum ObjectFlags : byte
    {
        None,

        Directory   = 1 << 0,
        Interrupted = 1 << 1,
        System      = 1 << 2,
        Hidden      = 1 << 3,

        SysDir = System | Directory
    }
}