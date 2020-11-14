using IkenFeline.LogManager;
using Mono.Cecil;
using MonoMod;
using MonoMod.DebugIL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline.Loader
{
    public class RuntimeMonoModder : MonoModder
    {
        string assemblyName;
        public RuntimeMonoModder(AssemblyDefinition assembly, string assemblyName)
        {
            Module = assembly.MainModule;
            this.assemblyName = assemblyName;

            var ass = Path.GetFileNameWithoutExtension(assemblyName);
            var ext = Path.GetExtension(assemblyName);

            OutputPath = Path.Combine(Paths.ExecutableDirectory, $"{ass}.Modded{ext}");
            Logger.Log(assemblyName);

            DebugSymbolOutputFormat = DebugSymbolFormat.PDB;
        }

        public void PerformPatches(HashSet<string> patches)
        {
            Read();

            foreach (var patch in patches)
            {
                ReadMod(patch);
            }

            MapDependencies();

            Logger.Log($"Found {Mods.Count} mods");

            PatchRefs();

            AutoPatch();

            Write();
        }

        public override void Dispose()
        {
            Module = null;
            base.Dispose();
        }
    }
}
