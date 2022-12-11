﻿namespace FileSystemNS
{
    public enum FSResult : byte // New values should be added to FSResultExt.cs
    {
        None,
        Success,

        NameWasNull,
        NameWasEmpty,
        NameExceededMaxLength,
        NameHadForbiddenChar,
        NameIsReserved,
        NameWasTaken,
        NameWasNotFound,

        FSIsFull
    }
}
