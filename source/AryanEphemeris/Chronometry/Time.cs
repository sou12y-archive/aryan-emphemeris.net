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
    public struct Time
    {
        public Time(double totalSeconds, TimeScale scale = TimeScale.Tdb)
        {
            var ds = DivRem(totalSeconds + SecondsPerHalfDay, SecondsPerDay);
            Day = (int)ds.quotient + D2000;
            TimeOfDay = ds.remainder;
            Scale = scale;
        }

        public Time(int day, double timeOfDay, TimeScale scale = TimeScale.Utc)
        {
            Day = day;
            TimeOfDay = timeOfDay;
            Scale = scale;
        }

        public int Day { get; }

        public double TimeOfDay { get; }

        public TimeScale Scale { get; }

        public double TotalSeconds
        {
            get { return (double)(Day - D2000) * SecondsPerDay - SecondsPerHalfDay + TimeOfDay; }
        }

        public Time ToAtomicTime(TimeScale scale)
        {
            if (!IsAtomicTimeScale(scale))
                throw new ArgumentException(InvalidTimeScale);
            if (scale == Scale)
                return new Time(Day, TimeOfDay, Scale);

            /*
            Time Conversion Flow:
            UTC ⇄ TAI ⇄ TDT ⇄ TDB
            */
            return scale > Scale ? ConvertAtomicTimeForward(scale) :
                throw new NotSupportedException(BackwardTransformationNotSupported);
        }

        public Time ToEarthRotationTime(TimeScale scale)
        {
            if (!IsEarthRotationTimeScale(scale))
                throw new ArgumentException(InvalidTimeScale);
            if (scale == Scale)
                return new Time(Day, TimeOfDay, Scale);

            /*
            Time Conversion Flow:
            UT1 ⇄ UTC ⇄ TSD
            */
            return scale > Scale ? ConvertEarthRotationTimeForward(scale) :
                throw new NotSupportedException(BackwardTransformationNotSupported);
        }

        private Time ConvertAtomicTimeForward(TimeScale newScale)
        {
            var currentScale = Scale;
            var seconds = TotalSeconds;
            var leapSecond = AryanKernel.GetLeapSecond();

            if (currentScale != newScale && currentScale == TimeScale.Utc)
            {
                // Convert UTC to TAI.
                seconds += leapSecond.GetDeltaTAI(Day);
                currentScale = TimeScale.Tai;
            }

            if (currentScale != newScale && currentScale == TimeScale.Tai)
            {
                // Convert TAI to TDT.
                seconds += DeltaTDT;
                currentScale = TimeScale.Tdt;
            }

            if (currentScale != newScale && currentScale == TimeScale.Tdt)
            {
                // Convert TDT to TDB.
                var g = M[0] + M[1] * seconds;
                seconds += K * Math.Sin(g + EB * Math.Sin(g));
                currentScale = TimeScale.Tdb;
            }

            if (currentScale != newScale)
                throw new ArgumentException(TimeScaleConversionNotSupported);

            return new Time(seconds, newScale);
        }

        private Time ConvertEarthRotationTimeForward(TimeScale newScale)
        {
            var currentScale = Scale;
            var seconds = TotalSeconds;

            if (currentScale != newScale && currentScale == TimeScale.Utc)
            {
                // Convert UTC to TSD.
                currentScale = TimeScale.Tsd;
                throw new NotImplementedException();
            }

            if (currentScale != newScale)
                throw new ArgumentException(TimeScaleConversionNotSupported);

            return new Time(seconds, newScale);
        }

        private static bool IsAtomicTimeScale(TimeScale scale)
        {
            return scale == TimeScale.Utc || scale == TimeScale.Tai || scale == TimeScale.Tdt || scale == TimeScale.Tdb;
        }

        private static bool IsEarthRotationTimeScale(TimeScale scale)
        {
            return scale == TimeScale.Ut1 || scale == TimeScale.Utc || scale == TimeScale.Tsd;
        }
    }
}