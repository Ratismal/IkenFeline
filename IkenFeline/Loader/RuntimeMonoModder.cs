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

            Logger.Log($"[Main] Found {Mods.Count} mods");

            PatchRefs();

            AutoPatch();

            Write();
        }

        static byte[] loadFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            byte[] buffer = new byte[(int)fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();

            return buffer;
        }

        public void LoadOnDomain(AppDomain domain)
        {
            var originalPath = Path.Combine(Paths.ExecutableDirectory, $"{assemblyName}");
            var tempPath = Path.Combine(Paths.ExecutableDirectory, $"__{assemblyName}");
            File.Move(originalPath, tempPath);
            File.Move(OutputPath, originalPath);

            Assembly.Load(assemblyName);

            File.Move(originalPath, OutputPath);
            File.Move(tempPath, originalPath);
            // Logger.Log(Module.Assembly.FullName);
            // var n = AssemblyName.GetAssemblyName(OutputPath);
            // Logger.Log(n.FullName);

            // var ass = AssemblyDefinition.ReadAssembly(OutputPath);
            // Logger.Log(ass.FullName);

            // var ass2 = Assembly.Load("LittleWitch");
            // Logger.Log(ass2.FullName + " " + ass2.Location);


            // var ass3 = Assembly.LoadFile(Path.Combine(Paths.ExecutableDirectory, "LittleWitch.dll"));
            // Logger.Log(ass3.FullName + " " + ass3.Location);
            // var rawAssembly = loadFile(OutputPath);
            // var rawSymbolStore = loadFile(OutputPath.Replace(".dll", ".pdb"));

            // var ass = Assembly.Load(rawAssembly, rawSymbolStore);
            // ass.GetName().SetPublicKey(Module.Assembly.Name.PublicKey);
            // domain.Load(rawAssembly, rawSymbolStore);
        }

        public override void Dispose()
        {
            Module = null;
            base.Dispose();
        }
    }
}
