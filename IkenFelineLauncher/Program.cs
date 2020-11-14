using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFelineLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing IkenFeline");
            IkenFeline.IkenFeline.Initialize();
            IkenFeline.IkenFeline.InitiatePatching();
            if (args.Length > 0 && args[0] == "--nosteam")
            {
                Console.WriteLine("Initializing without Steam support.");
                Steamless.Start();
            } else
            {
                Steam.Start();
            }
        }
    }
}
