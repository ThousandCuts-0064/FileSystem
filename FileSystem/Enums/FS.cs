using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemNS
{
    [Flags]
    public enum FS
    {
        None       = 0,
        BootByte   = 1 << 0,
        FSInfo     = 1 << 1,
        BootSector = BootByte | FSInfo,
        BitMap     = 1 << 2,
        BadSectors = 1 << 3,
        Root       = 1 << 4,
        All        = (1 << 5) - 1
    }
}
