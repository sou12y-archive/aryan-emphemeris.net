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

namespace AryanEphemeris
{
    public enum EphemerisComponent
    {
        Mercury = 0,
        Venus = 1,
        Earth = 2,
        Mars = 3,
        Jupiter = 4,
        Saturn = 5,
        Uranus = 6,
        Neptune = 7,
        Pluto = 8,
        Moon = 9,
        Sun = 10,
        SolarSystemBarycenter = 11,
        EarthMoonBarycenter = 12,
        EarthNutation = 13,
        MoonLibration = 14,
        MoonAngularVelocity = 15,
        TTMinusTDB = 16
    }
}