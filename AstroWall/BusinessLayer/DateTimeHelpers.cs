using System;
using System.Linq;

namespace AstroWall
{
    /// <summary>
    /// Helpers for working with DateTime structs.
    /// </summary>
    internal class DateTimeHelpers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeHelpers"/> class.
        /// Only static methods, should not be called.
        /// </summary>
        private DateTimeHelpers()
        {
        }

        /// <summary>
        /// Finds all dates that exists in the first array, but not in the second.
        /// </summary>
        /// <param name="dts1"></param>
        /// <param name="dts2"></param>
        /// <returns>DateTime[]</returns>
        internal static DateTime[] DatesDiff(DateTime[] dts1, DateTime[] dts2)
        {
            return dts1.Where(dt1 => !dts2.Any(dt2 => DTEquals(dt1, dt2))).Cast<DateTime>().ToArray();
        }

        /// <summary>
        /// Custom equals function that only compares the year, month and day, but not time.
        /// </summary>
        internal static bool DTEquals(DateTime dt1, DateTime dt2)
        {
            bool isEqual = true;
            if (!(dt1.Year == dt2.Year))
            {
                isEqual = false;
            }

            if (!(dt1.Day == dt2.Day))
            {
                isEqual = false;
            }

            if (!(dt1.Month == dt2.Month))
            {
                isEqual = false;
            }

            return isEqual;
        }
    }
}