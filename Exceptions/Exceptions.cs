using System;

namespace ExceptionsNS
{
    public class Exceptions
    {
        public static string CannotBeNull => "Value cannot be null";
        public static string CannotBeNegative => "Number cannot be negative";
        public static string DestArrNotLongEnough => "Destination array was not long enough.";
        public static string CapacityLessThanSize => "Capacity was less than the current size.";
        public static string ArrCapacityExceeded => $"Array legnth cannot exceed {nameof(Int32)}.{nameof(int.MaxValue)}.";
    }
}
