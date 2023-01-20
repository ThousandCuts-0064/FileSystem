using System;

namespace FileSystemNS
{
    [Flags]
    internal enum ObjectFlags : byte
    {
        None,

        Directory   = 1 << 0,
        System      = 1 << 1,
        Hidden      = 1 << 2,

        SysDir = System | Directory
    }
}