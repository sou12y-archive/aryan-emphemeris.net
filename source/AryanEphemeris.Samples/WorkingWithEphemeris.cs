using System;
using System.IO;

namespace AryanEphemeris.Samples
{
    public class WorkingWithEphemeris
    {
        public static void ConvertTextToBinaryEphemeris()
        {
            var textEphemeris = Path.Combine(FilePaths.AdditionalDataPath, "text.430");
            var binaryEphemeris = Path.Combine(FilePaths.DataPath, "binary.430");

            using (var builder = new EphemerisBuilder(textEphemeris, binaryEphemeris))
                builder.Build();
        }

        public static void InterpolateEphemeris()
        {
            var ephemerisPath = Path.Combine(FilePaths.DataPath, "binary.430");

            AryanKernel.LoadEphemeris(ephemerisPath);
            var ephemeris = AryanKernel.GetEphemeris();
            var coordinates = ephemeris.Interpolate(2451545.0, EphemerisComponent.Sun);

            Console.WriteLine("Position of Sun at 2451545.0 is: ");
            Console.WriteLine($"x = \t {coordinates[0]}");
            Console.WriteLine($"y = \t {coordinates[1]}");
            Console.WriteLine($"z = \t {coordinates[2]}");
            Console.WriteLine("Velocity of Sun at 2451545.0 is: ");
            Console.WriteLine($"x = \t {coordinates[3]}");
            Console.WriteLine($"y = \t {coordinates[4]}");
            Console.WriteLine($"z = \t {coordinates[5]}");
            Console.ReadLine();
        }
    }
}