using System;
using System.Linq;

namespace AstroWall
{
    public class DateTimeHelpers
    {
        public DateTimeHelpers()
        {
        }


        /// <summary>
        /// Finds all dates that exists in the first array, but not in the second
        /// </summary>
        /// <param name="dts1"></param>
        /// <param name="dts2"></param>
        /// <returns></returns>
        public static DateTime[] datesDiff(DateTime[] dts1, DateTime[] dts2)
        {
            return dts1.Where(dt1 => !dts2.Any(dt2 => DTEquals(dt1, dt2))).Cast<DateTime>().ToArray();
        }

        public static bool DTEquals(DateTime dt1, DateTime dt2)
        {
            bool isEqual = true;
            if (!(dt1.Year == dt2.Year)) isEqual = false;
            if (!(dt1.Day == dt2.Day)) isEqual = false;
            if (!(dt1.Month == dt2.Month)) isEqual = false;
            return isEqual;
        }
    }
}

