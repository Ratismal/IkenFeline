using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline
{
    class Paths
    {
        public static string ExecutableDirectory { get; private set; }
        public static string WorkingDirectory { get; private set; }
        public static string LogDirectory { get; private set; }
        public static string ModDirectory { get; private set; }

        public static void ObtainPaths()
        {
            string location = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;

            ExecutableDirectory = Path.GetDirectoryName(location).Replace("file:\\", "");
            System.Diagnostics.Debug.WriteLine(ExecutableDirectory);

            WorkingDirectory = Path.Combine(ExecutableDirectory, "IkenFeline");
            Directory.CreateDirectory(WorkingDirectory);

            LogDirectory = Path.Combine(WorkingDirectory, "log");
            Directory.CreateDirectory(LogDirectory);

            ModDirectory = Path.Combine(WorkingDirectory, "mods");
            Directory.CreateDirectory(ModDirectory);

        }
    }
}
