namespace FileSystemNS
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

        FormatNotSupported,
        FormatNotSpecified,
        FormatMismatch,

        RootHasNoParent,
        NotEnoughSpace,
        BadSectorFound,
        RootCorrupted,
    }
}
