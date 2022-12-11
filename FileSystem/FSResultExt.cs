using System;

namespace FileSystemNS
{
    public static class FSResultExt
    {
        public static string ToMessage(this FSResult result)
        {
            switch (result)
            {
                case FSResult.None: throw new InvalidOperationException($"A value different of {FSResult.None} was expected.");

                case FSResult.Success:               return "Success!";
                case FSResult.NameWasNull:           return "Name was null.";
                case FSResult.NameWasEmpty:          return "Name was empty.";
                case FSResult.NameExceededMaxLength: return "Name exceeded max length.";
                case FSResult.NameHadForbiddenChar:  return "Name had a forbidden char.";
                case FSResult.NameIsReserved:        return "Name is reserved by the file system.";
                case FSResult.NameWasTaken:          return "Name was already taken.";
                case FSResult.NameWasNotFound:       return "Name was not found.";

                case FSResult.FSIsFull:              return "The file system is already full.";

                default: throw new NotImplementedException();
            }
        }
    }
}
