using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnvironmentSensorDashboard.Data
{
    public static class DataRelatedExtensionMethods
    {
        private static DateTime SQLMinimumSafeDate = new DateTime(1753,1,1);
        private static DateTime SQLMaximumSafeDate = new DateTime(9999,12,31);

        public static DateTime ToSQLSafeDate(this DateTime thisDate)
        {
            if (thisDate <= SQLMinimumSafeDate) {
                return SQLMinimumSafeDate;
            }

            if (thisDate >= SQLMaximumSafeDate) {
                return SQLMaximumSafeDate;
            }

            return thisDate;
        }


    }
}
