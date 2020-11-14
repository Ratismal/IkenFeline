using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline.Mod
{
    public class ModManifest
    {
        public string ModId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Main { get; set; }
        public List<string> Patches { get; set; }
        public string Path { get; set; }
        public string MainClass { get; set; }
    }
}
