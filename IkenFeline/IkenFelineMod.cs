using IkenFeline.LogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline
{
    public abstract class IkenFelineMod
    {
        public string ModName { get; set; }
        public string ModId { get; set; }
        public Version Version { get; set; } = new Version(0, 1);
        public ModLogger Logger;

        public IkenFelineMod()
        {
            Logger = new ModLogger(this);
        }

        public virtual void Load() { }
        public virtual void PostLoad() { }
        public virtual void PreGameLoad() { }
        public virtual void PostGameLoad() { }
        public virtual void Update() { }
    }
}
