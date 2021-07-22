using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnvironmentSensorDashboard
{
    public static class ParserExtensions
    {
        public static int ToInt(this string thisString)
        {
            int returnMe = 0;

            if (int.TryParse(thisString, out returnMe))
            {
                return returnMe;
            }

            return 0;
        }

        public static double ToDouble(this string thisString)
        {
            double returnMe = 0;

            if (double.TryParse(thisString, out returnMe))
            {
                return returnMe;
            }

            return 0;
        }

        public static DateTime ToDateTime(this string thisDate)
        {
            // Check if we were given the "null" date value from a SQL database, and return the C# minvalue instead
            if (thisDate.Trim() == "1900-01-01 00:00:00.000")
            {
                return DateTime.MinValue;
            }

            DateTime returnMe = DateTime.MinValue;

            if (!DateTime.TryParse(thisDate, out returnMe))
            {
                returnMe = DateTime.MinValue;
            }

            // Again, check if we've managed to parse the SQL's minimum date and convert it
            if (returnMe == new DateTime(1900, 1, 1))
            {
                returnMe = DateTime.MinValue;
            }

            return returnMe;
        }

        public static bool ToBool(this string thisDatabaseValue)
        {
            if (string.IsNullOrEmpty(thisDatabaseValue))
            {
                return false;
            }
            else
            {
                bool parsedBool = false;
                bool.TryParse(thisDatabaseValue, out parsedBool);
                return parsedBool;
            }
        }


        public static List<int> ParseIDList(this string idlist, char delimiter)
        {
            List<int> returnMe = new List<int>();
            List<string> rawItems = idlist.Split(delimiter).ToList();

            foreach (string i in rawItems)
            {
                if (!string.IsNullOrEmpty(i))
                {
                    returnMe.Add(i.ToInt());
                }
            }

            return returnMe;

        }
    }
}
