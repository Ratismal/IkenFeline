using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline.LogManager
{
    public class ModLogger
    {
        IkenFelineMod mod;
        public ModLogger(IkenFelineMod mod)
        {
            this.mod = mod;
        }

        public void Log(string format, params object[] list)
        {
            format = $"[{mod.ModName}] {format}";
            Logger.LogDirect(format, list);
        }
    }
}
