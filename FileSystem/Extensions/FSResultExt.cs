using System;
using Text;

namespace FileSystemNS
{
    public static class FSResultExt
    {
        public static bool IsError(this FSResult result, Action<string> logError = null, string param = null) =>
            !result.IsSuccess(logError, param);
        public static bool IsSuccess(this FSResult result, Action<string> logError = null, string param = null)
        {
            if (result == FSResult.Success)
                return true;

            if (!(logError is null))
                logError(result.ToMessage(param));

            return false;
        }

        public static string ToMessage(this FSResult result, string param = null)
        {
            switch (result)
            {
                case FSResult.None: throw new InvalidOperationException($"A value different of {FSResult.None} was expected.");
                
                case FSResult.Success:               return "Success!";

                case FSResult.NameWasNull:           return Name(" was null.");
                case FSResult.NameWasEmpty:          return Name(" was empty.");
                case FSResult.NameExceededMaxLength: return Name(" exceeds max length.");
                case FSResult.NameHadForbiddenChar:  return Name(" has a forbidden char.");
                case FSResult.NameIsReserved:        return Name(" is reserved by the file system.");
                case FSResult.NameWasTaken:          return Name(" is already taken.");
                case FSResult.NameWasNotFound:       return Name(" was not found.");

                case FSResult.FormatNotSupported:    return $"The file format{(param is null ? "" : $" of \"{param}\"")} is not supported.";
                case FSResult.FormatNotSpecified:    return $"The file format was not specified.";
                case FSResult.FormatMismatch:        return $"The file format{(param is null ? "" : $" of \"{param}\"")} did not match.";

                case FSResult.RootHasNoParent:       return "Tried to access the parent of the root.";
                case FSResult.NotEnoughSpace:        return "There is not enough space in the file system.";
                case FSResult.BadSectorFound:        return "Bad sector was found.";

                default: throw new NotImplementedException();
            }

            string Name(string msg) => 
                (param.IsNullOrEmpty_()
                    ? nameof(Name)
                    : '\"' + param + '\"')
                        + msg;
        }
    }
}
