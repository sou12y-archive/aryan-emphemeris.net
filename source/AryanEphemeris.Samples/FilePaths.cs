using System.IO;
using System.Reflection;

namespace AryanEphemeris.Samples
{
    public class FilePaths
    {
        public static string DataPath
        {
            get
            {
                return Path.GetFullPath(
                       Path.Combine(
                           Path.GetDirectoryName(
                               Assembly.GetExecutingAssembly().Location),
                           @"..\..\..\..\..\data\"));
            }
        }

        public static string AdditionalDataPath
        {
            get { return Path.Combine(DataPath, "additional"); }
        }
    }
}