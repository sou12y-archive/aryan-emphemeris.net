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
using AryanEphemeris.Chronometry;
using System;
using System.IO;
using System.Reflection;

namespace AryanEphemerisTests
{
    public class KernelFixture : IDisposable
    {
        public KernelFixture()
        {
            DataPath = Path.GetFullPath(
                       Path.Combine(
                           Path.GetDirectoryName(
                               Assembly.GetExecutingAssembly().Location),
                           @"..\..\..\..\..\data\"));
            AdditionalDataPath = Path.Combine(DataPath, @"additional\");

            AryanKernel.LoadEphemeris(Path.Combine(DataPath, "binary.430"));
            AryanKernel.LoadLeapSecond(Path.Combine(DataPath, "text.leap"));

            Ephemeris = AryanKernel.GetEphemeris();
            LeapSecond = AryanKernel.GetLeapSecond();
        }

        public Ephemeris Ephemeris { get; }

        public LeapSecond LeapSecond { get; }

        public string DataPath { get; }

        public string AdditionalDataPath { get; }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}