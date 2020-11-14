using IkenFeline.Attributes;
using IkenFeline.Hooks;
using IkenFeline.Mod;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace IkenFeline.Loader
{
    class ModDefinition
    {
        public ModManifest manifest;
        public string path;
    }

    public sealed class ModLoader
    {
        public static Dictionary<string, ModManifest> ModList { get; private set; }
        internal static Dictionary<string, IkenFelineMod> Mods { get; set; }
        static List<ModDefinition> definitions { get; set; }
        static Dictionary<string, HashSet<string>> patchMap { get; set; }
        static HashSet<string> patchSet { get; set; }

        private static readonly HashSet<string> UnpatchableAssemblies =
                    new HashSet<string>(StringComparer.CurrentCultureIgnoreCase) { "mscorlib" };

        static IDeserializer deserializer;
        private static List<string> assemblyPaths;
        static Dictionary<string, Assembly> assemblies;

        public static void Initialize()
        {
            deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
            definitions = new List<ModDefinition>();
            Mods = new Dictionary<string, IkenFelineMod>();
            ModList = new Dictionary<string, ModManifest>();
            patchMap = new Dictionary<string, HashSet<string>>();
            patchSet = new HashSet<string>();
            assemblyPaths = new List<string>();
            assemblies = new Dictionary<string, Assembly>();

            LogManager.Logger.Log("Collecting mod list...");
            CollectMods();

            LogManager.Logger.Log("Collecting patches...");
            CollectPatches();

            if (patchMap.Count > 0)
            {
                LogManager.Logger.Log("Applying patches...");
                assemblyPaths = ModPatcher.Patch(patchMap);
            }

            LogManager.Logger.Log("Loading patched assemblies...");
            LoadAssemblies();
            
            LogManager.Logger.Log("Loading main mods...");
            LoadMods();

            LogManager.Logger.Log("Loading assets...");
            AssetLoader.LoadAssets();

            LogManager.Logger.Log("Triggering post-load event...");
            foreach (var mod in Mods.Values)
            {
                mod.PostLoad();
            }

            LogManager.Logger.Log("Registering hooks...");
            ModHooks.CreateHooks();
        }

        static void CollectMods()
        {
            var directories = Directory.GetDirectories(Paths.ModDirectory);

            foreach (var directory in directories)
            {
                var manifestPath = Path.Combine(directory, "manifest.yaml");
                if (File.Exists(manifestPath))
                {
                    var manifest = deserializer.Deserialize<ModManifest>(File.ReadAllText(manifestPath));
                    manifest.Path = directory;
                    if (!string.IsNullOrWhiteSpace(manifest.ModId) && !string.IsNullOrWhiteSpace(manifest.Main) && !string.IsNullOrWhiteSpace(manifest.MainClass))
                    {
                        definitions.Add(new ModDefinition() { manifest = manifest, path = directory });
                        ModList.Add(manifest.ModId, manifest);
                    }
                }
            }
        }

        static void LoadMods()
        {
            foreach (var definition in definitions)
            {
                var mainAssemblyPath = Path.Combine(definition.path, definition.manifest.Main);
                var type = FindModType(mainAssemblyPath, definition.manifest.MainClass);
                if (type != null)
                {
                    IkenFelineMod mod = (IkenFelineMod)Activator.CreateInstance(type, definition.manifest);

                    Mods.Add(mod.Manifest.ModId, mod);

                    mod.Load();
                }
            }
        }

        static void CollectPatches()
        {
            foreach (var mod in ModList.Values)
            {
                if (mod.Patches != null)
                {
                    foreach (var patch in mod.Patches)
                    {
                        var patchPath = Path.Combine(mod.Path, patch);
                        var assemblyName = GetAssemblyFromFile(patchPath);
                        if (!patchMap.ContainsKey(assemblyName))
                        {
                            patchMap.Add(assemblyName, new HashSet<string>());
                        }
                        patchMap[assemblyName].Add(patchPath);
                        patchSet.Add(patchPath);
                    }
                }
            }
        }

        static void LoadAssemblies()
        {
            foreach (var name in assemblyPaths)
            {
                var path = Path.Combine(Paths.ExecutableDirectory, name);
                var assembly = Assembly.LoadFrom(path);
                assemblies.Add(name, assembly);
            }
        }

        private static string GetAssemblyFromFile(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            try
            {
                using (var ass = AssemblyDefinition.ReadAssembly(file))
                {
                    foreach (var assRef in ass.MainModule.AssemblyReferences)
                    {
                        if (!UnpatchableAssemblies.Contains(assRef.Name) &&
                            (fileName.StartsWith(assRef.Name, StringComparison.InvariantCultureIgnoreCase)
                            || fileName.StartsWith(assRef.Name.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase)))
                        {
                            return assRef.Name + ".dll";
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        static Type FindModType(string path, string className)
        {
            Assembly assembly = null;
            foreach (var ass in patchMap.Keys)
            {
                if (patchMap[ass].Contains(path))
                {
                    assembly = assemblies[ass];
                }
            }
            if (assembly == null) { 
                assembly = Assembly.LoadFrom(path);
            }
            Type type = assembly.GetType(className);
            return type;
        }
    }
}
