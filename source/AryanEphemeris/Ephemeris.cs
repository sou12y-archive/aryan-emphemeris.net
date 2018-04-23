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
using static AryanEphemeris.Internal.ErrorMessages;

namespace AryanEphemeris
{
    public class Ephemeris : IKernel, IDisposable
    {
        private static readonly int[] DE430Components = new[] { 0, 1, 12, 3, 4, 5, 6, 7, 8, 9, 10, 13, 14, 15, 16 };

        private int dataOffset;
        private bool isFirstReading;
        private BinaryReader reader;

        private double recordSpan;
        private double[] coefficients;
        private Dictionary<string, double> constants;
        private Dictionary<EphemerisComponent, EphemerisRecordPointer> pointers;

        public Ephemeris() { }

        public string Version { get; private set; }

        public double StartEpoch { get; private set; }

        public double FinalEpoch { get; private set; }

        public double GetConstant(string name)
        {
            if (!constants.ContainsKey(name))
                throw new ArgumentException(ConstantNotFound, nameof(name));

            return constants[name];
        }

        public double[] Interpolate(double jdtdb, EphemerisComponent target, EphemerisComponent center = EphemerisComponent.SolarSystemBarycenter)
        {
            if (jdtdb < StartEpoch || jdtdb > FinalEpoch)
                throw new ArgumentOutOfRangeException(nameof(jdtdb), TimeOutOfRange);

            var targetCoordinates = new double[6];
            var centerCoordinates = new double[6];

            // Nutation, Libration, Angular Velocity and TT-TDB.
            if (target == EphemerisComponent.EarthNutation || target == EphemerisComponent.MoonLibration ||
                target == EphemerisComponent.MoonAngularVelocity || target == EphemerisComponent.TTMinusTDB)
                return InterpolateChebyshev(jdtdb, target);

            // Moon at Earth Geocenter.
            if (target == EphemerisComponent.Moon && center == EphemerisComponent.Earth)
                targetCoordinates = InterpolateChebyshev(jdtdb, target);

            // Earth at Moon Geocenter.
            else if (target == EphemerisComponent.Earth && center == EphemerisComponent.Moon)
            {
                targetCoordinates = InterpolateChebyshev(jdtdb, center);
                for (var axis = 0; axis < targetCoordinates.Length; axis++)
                    targetCoordinates[axis] = -targetCoordinates[axis];
            }

            // Anything that relates to either Earth or Moon.
            else if (target == EphemerisComponent.Earth || target == EphemerisComponent.Moon ||
                     center == EphemerisComponent.Earth || center == EphemerisComponent.Moon)
            {
                var emrat = 1.0 / (1.0 + constants["EMRAT"]);
                if (target == EphemerisComponent.Moon || center == EphemerisComponent.Moon)
                    emrat -= 1.0;

                var embCoordinates = InterpolateChebyshev(jdtdb, EphemerisComponent.EarthMoonBarycenter);
                var moonCoordinates = InterpolateChebyshev(jdtdb, EphemerisComponent.Moon);

                // Earth/Moon at Planetocenter/SSB.
                if (target == EphemerisComponent.Earth || target == EphemerisComponent.Moon)
                {
                    if (center != EphemerisComponent.SolarSystemBarycenter)
                        targetCoordinates = InterpolateChebyshev(jdtdb, center);
                    for (var axis = 0; axis < targetCoordinates.Length; axis++)
                        targetCoordinates[axis] = (embCoordinates[axis] - emrat * moonCoordinates[axis]) - targetCoordinates[axis];
                }

                // Planet/SSB at Earth/Moon Geocenter.
                else
                {
                    if (target != EphemerisComponent.SolarSystemBarycenter)
                        targetCoordinates = InterpolateChebyshev(jdtdb, target);
                    for (var axis = 0; axis < targetCoordinates.Length; axis++)
                        targetCoordinates[axis] = targetCoordinates[axis] - (embCoordinates[axis] - emrat * moonCoordinates[axis]);
                }
            }

            // Anything that relates to bodies other than Earth/Moon.
            else
            {
                if (target != EphemerisComponent.SolarSystemBarycenter)
                    targetCoordinates = InterpolateChebyshev(jdtdb, target);
                if (center != EphemerisComponent.SolarSystemBarycenter)
                    centerCoordinates = InterpolateChebyshev(jdtdb, center);
                for (var axis = 0; axis < targetCoordinates.Length; axis++)
                    targetCoordinates[axis] -= centerCoordinates[axis];
            }

            for (var axis = 0; axis < targetCoordinates.Length; axis++)
                targetCoordinates[axis] /= constants["AU"];

            return targetCoordinates;
        }

        private double[] InterpolateChebyshev(double tdb, EphemerisComponent component)
        {
            if (!pointers.ContainsKey(component) || pointers[component].CoefficientSetCount == 0)
                throw new ArgumentException(EphemerisComponentNotAvailable, nameof(component));

            var recordPointer = pointers[component];
            var interval = (tdb - StartEpoch) / recordSpan;
            var segment = (int)Math.Floor(interval);

            // Use previous segment if time is final epoch.
            if (Math.Abs(tdb - FinalEpoch) < double.Epsilon)
                segment--;

            var subInterval = (interval - segment) * recordPointer.CoefficientSetCount;
            var subSegment = (int)Math.Floor(subInterval);
            var timeSegment = 2.0 * (subInterval - subSegment) - 1.0;

            // Load segment if not already loaded.
            if (isFirstReading || tdb < coefficients[0] || tdb >= coefficients[1])
            {
                reader.BaseStream.Seek(dataOffset + segment * coefficients.Length * sizeof(double), SeekOrigin.Begin);
                for (var i = 0; i < coefficients.Length; i++)
                    coefficients[i] = reader.ReadDouble();

                isFirstReading = false;
            }

            // Default is set to cartesian axes count.
            var coordinateCount = 3;
            if (component == EphemerisComponent.EarthNutation)
                coordinateCount = 2;
            else if (component == EphemerisComponent.TTMinusTDB)
                coordinateCount = 1;

            var coordinates = new double[coordinateCount * 2];
            for (var axis = 0; axis < coordinateCount; axis++)
            {
                var p0 = 1.0;
                var p1 = timeSegment;
                var p2 = 0.0;
                var v0 = 0.0;
                var v1 = 1.0;
                var v2 = 0.0;
                var offset = recordPointer.CoefficientCountPerSet * (coordinateCount * subSegment + axis) + recordPointer.Offset;

                coordinates[axis] = coefficients[offset];
                for (var i = 1; i < recordPointer.CoefficientCountPerSet; i++)
                {
                    coordinates[axis] += coefficients[offset + i] * p1;
                    coordinates[axis + coordinateCount] += coefficients[offset + i] * v1;

                    p2 = 2.0 * timeSegment * p1 - p0;
                    v2 = 2.0 * timeSegment * v1 - v0 + 2 * p1;
                    p0 = p1;
                    p1 = p2;
                    v0 = v1;
                    v1 = v2;
                }
                coordinates[axis + coordinateCount] *= (2 * recordPointer.CoefficientSetCount) / recordSpan;
            }

            return coordinates;
        }

        public void LoadFrom(string file)
        {
            /*
            |-------------------------------------------------------|
            |   Data Length             |   Value                   |
            |-------------------------------------------------------|
            |   int32                   |   Header Size             |
            |   string                  |   Version                 |
            |   float64                 |   Start Epoch             |
            |   float64                 |   Final Epoch             |
            |   float64                 |   Record Span             |
            |   int32                   |   Constant Count    (i)   |
            |   int32                   |   Component Count   (j)   |
            |   int32                   |   Coefficient Count (k)   |
            |-------------------------------------------------------|
            |   (string + float32) × i  |   Constants               |
            |   int32 × 3 × j           |   Coefficient Pointers    |  
            |-------------------------------------------------------|
            |   float64 × k             |   Data Records            |
            |-------------------------------------------------------|
            */
            reader = new BinaryReader(File.OpenRead(file));

            dataOffset = reader.ReadInt32();
            Version = reader.ReadString();
            StartEpoch = reader.ReadDouble();
            FinalEpoch = reader.ReadDouble();
            recordSpan = reader.ReadDouble();

            var constantCount = reader.ReadInt32();
            var componentCount = reader.ReadInt32();
            var coefficientCount = reader.ReadInt32();

            constants = new Dictionary<string, double>(constantCount);
            for (var i = 0; i < constantCount; i++)
                constants.Add(reader.ReadString(), reader.ReadDouble());

            pointers = new Dictionary<EphemerisComponent, EphemerisRecordPointer>(15);
            for (var i = 0; i < componentCount; i++)
            {
                var o = reader.ReadInt32() - 1;
                var c = reader.ReadInt32();
                var s = reader.ReadInt32();
                pointers.Add((EphemerisComponent)DE430Components[i], new EphemerisRecordPointer(o, s, c));
            }

            coefficients = new double[coefficientCount];
            isFirstReading = true;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    reader.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}