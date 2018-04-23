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
using System.IO;
using System.Linq;
using static AryanEphemeris.Internal.ErrorMessages;

namespace AryanEphemeris
{
    /// <summary>
    /// |-------------------------------------------------------|
    /// |   Data Length             |   Value                   |
    /// |-------------------------------------------------------|
    /// |   int32                   |   Header Size             |
    /// |   string                  |   Version                 |
    /// |   float64                 |   Start Epoch             |
    /// |   float64                 |   Final Epoch             |
    /// |   float64                 |   Record Span             |
    /// |   int32                   |   Constant Count    (i)   |
    /// |   int32                   |   Component Count   (j)   |
    /// |   int32                   |   Coefficient Count (k)   |
    /// |-------------------------------------------------------|
    /// |   (string + float32) × i  |   Constants               |
    /// |   int32 × 3 × j           |   Coefficient Pointers    |  
    /// |-------------------------------------------------------|
    /// |   float64 × k             |   Data Records            |
    /// |-------------------------------------------------------|
    /// </summary>
    public class EphemerisBuilder : IDisposable
    {
        private const string Version = "0.1";

        private int headerSize;
        private string version;

        private double startEpoch;
        private double finalEpoch;
        private double recordSpan;

        private int constantCount;
        private int componentCount;
        private int coefficientCount;

        private string[] constantNames;
        private double[] constantValues;
        private int[][] pointers;
        private double[] coefficients;

        private BinaryWriter writer;
        private StreamReader reader;

        public EphemerisBuilder(string input, string output)
        {
            reader = new StreamReader(File.OpenRead(input));
            writer = new BinaryWriter(File.OpenWrite(output));
            version = Version;
        }

        public void Build()
        {
            try
            {
                WriteHeader();
                WriteRecords();
            }
            catch (Exception e)
            {
                throw new InvalidDataException(InvalidEphemerisData, e);
            }
        }

        private void WriteHeader()
        {
            ReadGroup1000();
            ReadGroup1030();
            ReadGroup1040();
            ReadGroup1041();
            ReadGroup1050();

            /*
            Allocate 4 bytes for writing header size.
            We will come back to this position and set the value after writing the actual header.
            */
            writer.Write(headerSize);

            writer.Write(version);
            writer.Write(startEpoch);
            writer.Write(finalEpoch);
            writer.Write(recordSpan);
            writer.Write(constantCount);
            writer.Write(componentCount);
            writer.Write(coefficientCount);

            // Write constants.
            for (var i = 0; i < constantCount; i++)
            {
                writer.Write(constantNames[i]);
                writer.Write(constantValues[i]);
            }

            // Write data pointers.
            for (var c = 0; c < componentCount; c++)
                for (var r = 0; r < 3; r++)
                    writer.Write(pointers[r][c]);

            /*
             * Get current position in stream and write it as header size
             * at the top of the file where we allocated memory for it earlier.
             */
            headerSize = (int)writer.BaseStream.Position;
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(headerSize);

            // Move to end of the header.
            writer.Seek(headerSize, SeekOrigin.Begin);
        }

        private void WriteRecords()
        {
            MoveToGroup("1070");

            var startTime = startEpoch;
            coefficients = new double[coefficientCount];
            while (!reader.EndOfStream)
            {
                // Ignore record information and move to the actual record.
                IgnoreLine();

                // Perform each task by offset of 3 as each line contains 3 coefficients.
                var lineCount = (int)Math.Ceiling(coefficientCount / 3.0);
                for (var i = 0; i < lineCount; i++)
                {
                    var doubleArray = ReadAsDoubleArray();
                    for (var j = 0; j < doubleArray.Length; j++)
                    {
                        var k = 3 * i + j;
                        if (k < coefficientCount)
                            coefficients[k] = doubleArray[j];
                    }
                }

                // Write coefficients ignoring duplicate records.
                if (Math.Abs(startTime - coefficients[0]) < double.Epsilon)
                {
                    for (var i = 0; i < coefficients.Length; i++)
                        writer.Write(coefficients[i]);

                    startTime = coefficients[1];
                }

                // We reached the last record.
                if (Math.Abs(coefficients[1] - finalEpoch) < double.Epsilon)
                    break;
            }
        }

        private void ReadGroup1000()
        {
            coefficientCount = int.Parse(ReadAsStringArray()[3]);
        }

        private void ReadGroup1030()
        {
            MoveToGroup("1030");
            var array = ReadAsDoubleArray();
            startEpoch = array[0];
            finalEpoch = array[1];
            recordSpan = array[2];
        }

        private void ReadGroup1040()
        {
            MoveToGroup("1040");
            constantCount = ReadAsInt32Array()[0];
            constantNames = new string[constantCount];

            // Perform each task by offset of 10 as each line contains 10 names.
            var lineCount = (int)Math.Ceiling(constantCount / 10.0);
            for (var i = 0; i < lineCount; i++)
            {
                var array = ReadAsStringArray();
                for (var j = 0; j < array.Length; j++)
                {
                    var k = 10 * i + j;
                    if (k < constantCount)
                        constantNames[k] = array[j];
                }
            }
        }

        private void ReadGroup1041()
        {
            MoveToGroup("1041");

            // If constant names count are smaller than constant values ignore the additional constant values.
            constantCount = Math.Min(constantCount, ReadAsInt32Array()[0]);
            constantValues = new double[constantCount];

            // Perform each task by offset of 3 as each line contains 3 constants.
            var lineCount = (int)Math.Ceiling(constantCount / 3.0);
            for (var i = 0; i < lineCount; i++)
            {
                var array = ReadAsDoubleArray();
                for (var j = 0; j < array.Length; j++)
                {
                    var k = 3 * i + j;
                    if (k < constantCount)
                        constantValues[k] = array[j];
                }
            }
        }

        private void ReadGroup1050()
        {
            MoveToGroup("1050");

            pointers = new int[3][];
            for (var i = 0; i < pointers.Length; i++)
                pointers[i] = ReadAsInt32Array();
            componentCount = pointers[0].Length;
        }

        public static void Join(string directory, string headerFileName, string fileNamePrefix, string outputFileName)
        {
            File.WriteAllText(outputFileName, File.ReadAllText(Path.Combine(directory, headerFileName)));

            var textFiles = Directory.GetFiles(directory, $"{fileNamePrefix}*");
            /*
             * Sorting files by filenames will correctly order the ephemeris text data files.
             * Provided that file names are suffixed with <n/p><year> value.
             */
            Array.Sort(textFiles);
            foreach (var file in textFiles)
                File.AppendAllText(outputFileName, File.ReadAllText(file));
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    writer.Close();
                    reader.Close();
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion

        #region Helper Functions
        public int[] ReadAsInt32Array()
        {
            return ReadAsStringArray()
                .Select(s => int.Parse(s))
                .ToArray();
        }

        public double[] ReadAsDoubleArray()
        {
            return ReadAsStringArray()
                .Select(s => double.Parse(s.Replace('D', 'E')))
                .ToArray();
        }

        public string[] ReadAsStringArray()
        {
            return reader.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public void IgnoreLine()
        {
            if (!reader.EndOfStream)
                reader.ReadLine();
        }

        public void MoveToGroup(string group)
        {
            while (!reader.EndOfStream)
                if (reader.ReadLine().EndsWith(group))
                    break;

            IgnoreLine();
        }
        #endregion
    }
}