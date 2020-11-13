using IkenFeline.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline.Attributes
{
    public class IkenFelineModAttribute : Attribute
    {
        public string ModId;
        public string Name;
        public Version Version;
        public IkenFelineModAttribute(string modid, string name, string version)
        {
            ModId = modid;
            Name = name;
            Version = new Version(version);
        }
    }
}
