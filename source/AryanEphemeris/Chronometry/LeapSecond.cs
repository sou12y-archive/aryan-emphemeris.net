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
using System.Collections.Generic;
using System.IO;

namespace AryanEphemeris.Chronometry
{
    public class LeapSecond : IKernel
    {
        private int[] days;
        private int[] seconds;

        public LeapSecond() { }

        public int GetDeltaTAI(int day)
        {
            if (days.Length == 0 || seconds.Length == 0)
                return 0;
            if (day < days[0])
                return seconds[0] - 1;

            var index = Array.BinarySearch(days, day);
            if (index < 0)
                index = -(index + 2);

            return seconds[index];
        }

        public bool HasLeapSecond(int day)
        {
            return Array.BinarySearch(days, day) > 0;
        }

        public void LoadFrom(string file)
        {
            var lines = File.ReadAllLines(file);
            var days = new List<int>(lines.Length);
            var seconds = new List<int>(lines.Length);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#"))
                    continue;

                var array = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var j = double.Parse(array[0]);
                var d = int.Parse(array[1]);
                var m = int.Parse(array[2]);
                var y = int.Parse(array[3]);
                var s = int.Parse(array[4]);

                days.Add(new CalendarDateFormat(y, m, d).GetAsTime(TimeScale.Ut1).Day);
                seconds.Add(s);
            }

            this.days = days.ToArray();
            this.seconds = seconds.ToArray();
        }
    }
}