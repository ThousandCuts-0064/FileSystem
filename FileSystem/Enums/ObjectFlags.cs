using System;

namespace FileSystemNS
{
    [Flags]
    internal enum ObjectFlags : byte
    {
        None,
        Folder = 1 << 0,
        System = 1 << 1,
        Hidden = 1 << 2,

        SysFolder = System | Folder
    }
}
