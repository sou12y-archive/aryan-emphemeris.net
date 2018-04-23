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

using AryanEphemeris;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace AryanEphemerisTests
{
    public class EphemerisTest : IClassFixture<KernelFixture>
    {
        private readonly KernelFixture kernelFixture;
        private readonly List<TestRecord> testedRecords = new List<TestRecord>();

        public EphemerisTest(KernelFixture fixture)
        {
            kernelFixture = fixture;

            // Read test file and store the records in a collection.
            var lines = File.ReadAllLines(Path.Combine(fixture.AdditionalDataPath, "testpo.430"));
            var lineIndex = 0;
            while (!lines[lineIndex].StartsWith("EOT"))
                lineIndex++;

            lineIndex++;
            while (lineIndex < lines.Length - 1)
            {
                var array = lines[lineIndex].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                testedRecords.Add(new TestRecord
                {
                    Time = double.Parse(array[2]),
                    Target = (EphemerisComponent)int.Parse(array[3]) - 1,
                    Center = (EphemerisComponent)int.Parse(array[4]) - 1,
                    Axis = int.Parse(array[5]) - 1,
                    Coordinate = double.Parse(array[6])
                });
                lineIndex++;
            }
        }

        [Fact]
        public void InterpolateTest()
        {
            var ephemeris = kernelFixture.Ephemeris;

            foreach (var testedRecord in testedRecords)
            {
                // Interpolate ephemeris.
                var coordinates = ephemeris.Interpolate(testedRecord.Time, testedRecord.Target, testedRecord.Center);

                // Make sure difference between original value and interpolated value is 0.
                var delta = Math.Abs(coordinates[testedRecord.Axis] - testedRecord.Coordinate);

                // Special case for Moon libration to make delta = 0.
                if (testedRecord.Target == EphemerisComponent.MoonLibration)
                    delta /= (1.0 + 100.0 * Math.Abs(testedRecord.Time - ephemeris.GetConstant("JDEPOC")) / 365.25);

                Assert.True(delta < 1E-13, "Interpolation produced wrong results.");
            }
        }
    }

    public class TestRecord
    {
        public double Time { get; set; }

        public EphemerisComponent Target { get; set; }

        public EphemerisComponent Center { get; set; }

        public int Axis { get; set; }

        public double Coordinate { get; set; }
    }
}