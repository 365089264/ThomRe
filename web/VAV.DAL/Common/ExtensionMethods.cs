using System;

namespace VAV.DAL.Common
{
    public static class ExtensionMethods
    {
        public static DateTime GetFirstDayOfYear(this DateTime date)
        {
            return Convert.ToDateTime(date.Year.ToString() + "-1-1");
        }

        public static DateTime GetLastDayOfYear(this DateTime date)
        {
            return Convert.ToDateTime(date.Year.ToString() + "-12-31");
        }

        public static DateTime GetFirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0);
        }

        public static DateTime GetLastDayOfMonth(this DateTime date)
        {
            DateTime lastDay = GetFirstDayOfMonth(date).AddMonths(1).AddDays(-1);
            return new DateTime(lastDay.Year, lastDay.Month, lastDay.Day, 0, 0, 0, 0);
        }

        public static DateTime GetTheFirstDayOfWeek(this DateTime dt)
        {
            DateTime theMondayDate;
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    theMondayDate = dt;
                    break;
                case DayOfWeek.Tuesday:
                    theMondayDate = dt.AddDays(-1);
                    break;
                case DayOfWeek.Wednesday:
                    theMondayDate = dt.AddDays(-2);
                    break;
                case DayOfWeek.Thursday:
                    theMondayDate = dt.AddDays(-3);
                    break;
                case DayOfWeek.Friday:
                    theMondayDate = dt.AddDays(-4);
                    break;
                case DayOfWeek.Saturday:
                    theMondayDate = dt.AddDays(-5);
                    break;
                case DayOfWeek.Sunday:
                    theMondayDate = dt.AddDays(-6);
                    break;
                default:
                    theMondayDate = dt;
                    break;
            }
            return theMondayDate;
        }

        public static DateTime GetTheFirstDayOfQuarter(this DateTime dt)
        {
            int month = dt.Month;
            int year = dt.Year;
            DateTime date = DateTime.Parse(year + "-01-01");
            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    break;
                case 4:
                case 5:
                case 6:
                    date = DateTime.Parse(year + "-04-01");
                    break;
                case 7:
                case 8:
                case 9:
                    date = DateTime.Parse(year + "-07-01");
                    break;
                case 10:
                case 11:
                case 12:
                    date = DateTime.Parse(year + "-10-01");
                    break;
            }
            return date;
        }

    }
}
