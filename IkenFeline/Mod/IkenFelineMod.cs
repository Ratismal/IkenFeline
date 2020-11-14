using IkenFeline.LogManager;
using IkenFeline.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline
{
    public abstract class IkenFelineMod
    {
        public ModManifest Manifest { get; private set; }
        public ModLogger Logger;

        public string ModName { get { return Manifest.Name; } }
        public Version Version { get; private set; }

        public IkenFelineMod(ModManifest manifest)
        {
            Manifest = manifest;
            Logger = new ModLogger(this);
            Version = new Version(Manifest.Version);
        }

        public virtual void Load() { }
        public virtual void PostLoad() { }
        public virtual void PreGameLoad() { }
        public virtual void PostGameLoad() { }
        public virtual void Update() { }
    }
}
