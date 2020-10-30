using IkenFeline.LogManager;
using Mono.Cecil;
using MonoMod;
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

            OutputPath = Path.Combine(Paths.ExecutableDirectory, $"_{assemblyName}");
            Logger.Log(assemblyName);
        }

        public void PerformPatches(string modDirectory)
        {
            Read();

            ReadMod(modDirectory);

            foreach (var directory in Directory.GetDirectories(modDirectory, "*", SearchOption.AllDirectories))
            {
                ReadMod(directory);
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
