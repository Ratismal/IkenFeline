using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline.LogManager
{
    public static class Logger
    {
        public static TextWriter ConsoleOut { get; private set; }
        public static StreamWriter Writer { get; private set; }
        public static FileStream Stream { get; private set; }
        public static FileStream SinglefileStream { get; private set; }
        public static StreamWriter SinglefileWriter { get; private set; }

        private static bool Started = false;

        public static void Initialize()
        {
            ConsoleOut = Console.Out;

            try
            {
                var filename = String.Format("ikenfeline-{0:MM-dd-yy_H-mm-ss}.log", DateTime.Now);
                // var filename = "ikenfeline.log";
                Stream = new FileStream(Path.Combine(Paths.LogDirectory, filename), FileMode.OpenOrCreate, FileAccess.Write);
                Writer = new StreamWriter(Stream);
                var singlefilePath = Path.Combine(Paths.LogDirectory, "ikenfeline.log");
                File.Delete(singlefilePath);
                SinglefileStream = new FileStream(singlefilePath, FileMode.OpenOrCreate, FileAccess.Write);
                SinglefileWriter = new StreamWriter(SinglefileStream);

                Writer.AutoFlush = true;
                SinglefileWriter.AutoFlush = true;

                Console.SetOut(Writer);
                Started = true;
            }
            catch
            {

            }

            Log("=======================\n| IKENFELINE\n| {0:yy-MM-dd H:mm:ss}\n=======================\n", DateTime.Now);
            Log("Initialized logger.");
        }

        public static void Dispose()
        {
            if (Started)
            {
                Console.SetOut(ConsoleOut);
                Writer.Close();
                Stream.Close();
                SinglefileWriter.Close();
                SinglefileStream.Close();
                Started = false;
            }
        }

        public static void Log(string format, params object[] list)
        {
            Console.WriteLine(format, list);
            SinglefileWriter.WriteLine(format, list);
            System.Diagnostics.Debug.WriteLine(format, list);
        }
    }
}
