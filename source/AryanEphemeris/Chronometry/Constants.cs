/***************************************************************************************************
 * Aryan Ephemeris
 * Copyright © 2018, Souvik Dey Chowdhury
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License
 * is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
 * or implied. See the License for the specific language governing permissions and limitations under
 * the License.
 **************************************************************************************************/

namespace AryanEphemeris.Chronometry
{
    internal static class Constants
    {
        // Calendar Constants
        public const int DaysPerYear = 365;
        public const int DaysPer4Years = 1461;
        public const int DaysPer100Years = 36524;
        public const int DaysPer400Years = 146097;
        public static readonly int[] DaysBeforeMonthIn365 = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
        public static readonly int[] DaysBeforeMonthIn366 = { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };

        // Time of Day Constants
        public const int SecondsPerMinute = 60;
        public const int SecondsPerHour = 3600;
        public const int SecondsPerHalfDay = 43200;
        public const int SecondsPerDay = 86400;

        // Julian Dates of 1 January XXXX 12:00:00 P.M A.D.
        public const double J0001 = 1721426.0;
        public const double J2000 = 2451545.0;
        public const double J2100 = 2488070.0;

        // Days past 1 January 1 A.D.
        public const int D0001 = (int)(J0001 - J0001);
        public const int D2000 = (int)(J2000 - J0001);
        public const int D2100 = (int)(J2100 - J0001);

        // Ephemeris Time Constants
        public const double DeltaTDT = 32.184;
        public const double K = 0.001657;
        public const double EB = 0.01671;
        public static readonly double[] M = new[] { 6.239996, 1.99096871E-7 };

        // Earth Rotation Time Constants
        public const double EraAtJ2000 = 0.7790572732640;
        public const double EraRate = 1.00273781191135448;
    }
}