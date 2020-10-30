using IkenFeline.Loader;
using IkenFeline.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline
{
    public class IkenFeline
    {
        public static void Initialize()
        {
            Paths.ObtainPaths();
            Logger.Initialize();
        }

        public static void InitiatePatching()
        {
            ModPatcher.Initialize();
        }
    }
}
