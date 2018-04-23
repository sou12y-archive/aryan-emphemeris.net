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

using System;
using static AryanEphemeris.AryanMath;
using static AryanEphemeris.Chronometry.Constants;
using static AryanEphemeris.Internal.ErrorMessages;

namespace AryanEphemeris.Chronometry
{
    public class CalendarDateFormat
    {
        public CalendarDateFormat(Time time)
        {
            var day = time.Day;
            var timeOfDay = time.TimeOfDay;

            // Year
            var y400 = day / DaysPer400Years;
            day -= y400 * DaysPer400Years;
            var y100 = Math.Min(3, day / DaysPer100Years);
            day -= y100 * DaysPer100Years;
            var y4 = Math.Min(24, day / DaysPer4Years);
            day -= y4 * DaysPer4Years;
            var y1 = Math.Min(3, day / DaysPerYear);
            day -= y1 * DaysPerYear;
            Year = (y400 * 400) + (y100 * 100) + (y4 * 4) + (y1 + 1);

            // Month
            day += 1;
            var before = IsLeapYear(Year) ? DaysBeforeMonthIn366 : DaysBeforeMonthIn365;
            Month = (day >> 5) + 1;
            while (day >= before[Month])
                Month++;

            // Day
            Day -= before[Month - 1] + 1;

            // Hour
            Hour = (int)(timeOfDay / SecondsPerHour);
            timeOfDay -= Hour * SecondsPerHour;

            // Minute
            Minute = (int)(timeOfDay / SecondsPerMinute);
            timeOfDay -= Minute * SecondsPerMinute;

            // Second
            Second = timeOfDay;
        }

        public CalendarDateFormat(int year, int month, int day, int hour = 0, int minute = 0, double second = 0.0)
        {
            var before = IsLeapYear(year) ? DaysBeforeMonthIn366 : DaysBeforeMonthIn365;

            if (year < 1 || month < 1 || month > 12 || day < 1 || day > before[month] - before[month - 1])
                throw new ArgumentOutOfRangeException(InvalidDateComponent);
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 60 || (second > 59 && (minute < 59 || hour < 23)))
                throw new ArgumentOutOfRangeException(InvalidTimeComponent);

            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
        }

        public int Year { get; }

        public int Month { get; }

        public int Day { get; }

        public int Hour { get; }

        public int Minute { get; }

        public double Second { get; }

        public Time GetAsTime(TimeScale scale)
        {
            var y = Year - 1;
            var d = IsLeapYear(Year) ? DaysBeforeMonthIn366 : DaysBeforeMonthIn365;

            var day = (365 * y) + (y / 4) - (y / 100) + (y / 400) + d[Month - 1] + (Day - 1);
            var timeOfDay = (Hour * SecondsPerHour) + (Minute * SecondsPerMinute) + Second;

            return new Time(day, timeOfDay, scale);
        }

        private static bool IsLeapYear(int year)
        {
            return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
        }
    }

    public class JulianDateFormat
    {
        public JulianDateFormat(double day)
        {
            Day = day;
        }

        public double Day { get; }

        public Time GetAsTime(TimeScale scale)
        {
            var julianSeconds = (Day - (J0001 - 0.5)) * SecondsPerDay;
            var ds = DivRem(julianSeconds, SecondsPerDay);

            var day = (int)ds.quotient;
            var timeOfDay = ds.remainder;

            return new Time(day, timeOfDay, scale);
        }
    }
}