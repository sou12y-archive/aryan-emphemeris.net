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

using AryanEphemeris.Chronometry;
using System;
using System.Collections.Generic;
using static AryanEphemeris.Internal.ErrorMessages;
using static AryanEphemeris.Internal.Validator;

namespace AryanEphemeris
{
    public interface IKernel
    {
        void LoadFrom(string file);
    }

    public static class AryanKernel
    {
        public const string JPLEphemeris = "JPLEphemeris";
        public const string IERSLeapSecond = "IERSLeapSecond";

        private static readonly Dictionary<string, IKernel> Kernels = new Dictionary<string, IKernel>();

        public static IKernel Get(string name)
        {
            ValidateEmptyString(name, nameof(name));
            if (!Kernels.ContainsKey(name))
                throw new ArgumentException(KernelNotLoaded, nameof(name));

            return Kernels[name];
        }

        public static Ephemeris GetEphemeris()
        {
            return (Ephemeris)Get(JPLEphemeris);
        }

        public static LeapSecond GetLeapSecond()
        {
            return (LeapSecond)Get(IERSLeapSecond);
        }

        public static IKernel Load(Type kernelType, string kernelName, string filePath)
        {
            ValidateNull(kernelType, nameof(kernelType));
            ValidateEmptyString(kernelName, nameof(kernelName));

            var kernel = (IKernel)Activator.CreateInstance(kernelType);
            kernel.LoadFrom(filePath);

            if (Kernels.ContainsKey(kernelName))
                Kernels[kernelName] = kernel;
            else
                Kernels.Add(kernelName, kernel);

            return kernel;
        }

        public static void LoadEphemeris(string filePath)
        {
            Load(typeof(Ephemeris), JPLEphemeris, filePath);
        }

        public static void LoadLeapSecond(string filePath)
        {
            Load(typeof(LeapSecond), IERSLeapSecond, filePath);
        }

        public static bool Unload(string name)
        {
            return Kernels.Remove(name);
        }

        public static bool UnloadEphemeris()
        {
            return Kernels.Remove(JPLEphemeris);
        }

        public static bool UnloadLeapSecond()
        {
            return Kernels.Remove(IERSLeapSecond);
        }
    }
}