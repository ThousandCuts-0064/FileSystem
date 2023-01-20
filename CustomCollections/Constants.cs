using System.Collections;

namespace CustomCollections
{
    public static class Constants
    {
        internal const string COLLECTION_DISPLAY = nameof(ICollection.Count) + ": {" + nameof(ICollection.Count) + "}";

        public const int ARRAY_MAX_LENGTH = int.MaxValue;
        public const int DEFAULT_SIZE = 4;
    }
}
