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
using static AryanEphemeris.Internal.ErrorMessages;
using static AryanEphemeris.Internal.Validator;

namespace AryanEphemeris
{
    public static class AryanMath
    {
        #region Mathematics
        public static (long quotient, double remainder) DivRem(double numerator, long denominator)
        {
            (long quotient, double remainder) result = (0L, 0.0);
            result.quotient = (int)(numerator / denominator);
            result.remainder = numerator % denominator;

            /// Floor the remainder to the half open interval [0, b).
            if (result.remainder < 0)
            {
                result.quotient--;
                result.remainder += denominator;
            }
            return result;
        }

        public static double NormalizeAngle(double x, double unit)
        {
            return x % unit;
        }
        #endregion

        #region Coordinate Systems
        public static double[] FromRectangularToSpherical(double[] coordinates)
        {
            return FromRectangularToSpherical(coordinates, 0);
        }

        public static double[] FromRectangularToSpherical(double[] coordinates, int offset)
        {
            ValidateNull(coordinates, nameof(coordinates));

            (var x, var y, var z) = ToRectangularCoordinates(coordinates, offset);
            var radius = Math.Sqrt(x * x + y * y + z * z);
            var inclination = Math.Acos(z / radius);
            var azimuth = Math.Atan(y / x);

            if (double.IsNaN(azimuth))
                azimuth = 0.0;

            return new[] { radius, inclination, azimuth };
        }

        public static double[] FromSphericalToRectangular(double[] coordinates)
        {
            return FromSphericalToRectangular(coordinates, 0);
        }

        public static double[] FromSphericalToRectangular(double[] coordinates, int offset)
        {
            ValidateNull(coordinates, nameof(coordinates));

            (var radius, var inclination, var azimuth) = ToSphericalCoordinates(coordinates, offset);
            var ri = radius * Math.Sin(inclination);
            var x = ri * Math.Cos(azimuth);
            var y = ri * Math.Sin(azimuth);
            var z = radius * Math.Cos(inclination);

            return new[] { x, y, z };
        }

        public static (double x, double y, double z) ToRectangularCoordinates(double[] coordinates)
        {
            return ToRectangularCoordinates(coordinates, 0);
        }

        public static (double x, double y, double z) ToRectangularCoordinates(double[] coordinates, int offset)
        {
            ValidateNull(coordinates, nameof(coordinates));
            if (coordinates.Length - offset % 3 != 0)
                throw new ArgumentOutOfRangeException(nameof(coordinates), CoordinatesCountOutOfRange);

            return (coordinates[offset + 0], coordinates[offset + 1], coordinates[offset + 2]);
        }

        public static (double radius, double inclination, double azimuth) ToSphericalCoordinates(double[] coordinates)
        {
            return ToSphericalCoordinates(coordinates, 0);
        }

        public static (double radius, double inclination, double azimuth) ToSphericalCoordinates(double[] coordinates, int offset)
        {
            ValidateNull(coordinates, nameof(coordinates));
            if (coordinates.Length - offset % 3 != 0)
                throw new ArgumentOutOfRangeException(nameof(coordinates), CoordinatesCountOutOfRange);

            return (coordinates[offset + 0], coordinates[offset + 1], coordinates[offset + 2]);
        }
        #endregion
    }
}