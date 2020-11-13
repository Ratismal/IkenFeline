using IkenFeline.Attributes;
using IkenFeline.Hooks;
using IkenFeline.LogManager;
using Mono.Cecil;
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
    public class ModPatcher
    {
        private static HashSet<string> patchSets;
        private static HashSet<string> assemblies;
        private static HashSet<string> patches;
        private static List<Type> modTypes;
        public static List<IkenFelineMod> Mods;

        public static string[] ResolveDirectories { get; set; }

        private static readonly HashSet<string> UnpatchableAssemblies =
            new HashSet<string>(StringComparer.CurrentCultureIgnoreCase) { "mscorlib" };

        public static void Initialize()
        {
            modTypes = new List<Type>();
            Logger.Log("Checking for patches...");
            ResolveDirectories = new string[] {
                Paths.ExecutableDirectory,
                Paths.ModDirectory
            };

            CollectPatchSets();
            CollectPatches();

            ResolveDirectories = ResolveDirectories.Concat(patchSets).ToArray();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AppDomainResolveHandler);

            ApplyPatches();

            Mods = new List<IkenFelineMod>();
            foreach (Type type in modTypes)
            {
                var mod = (IkenFelineMod)Activator.CreateInstance(type);
                IkenFelineModAttribute attribute = (IkenFelineModAttribute)type.GetCustomAttribute(typeof(IkenFelineModAttribute));
                mod.ModName = attribute.Name;
                mod.ModId = attribute.ModId;
                mod.Version = attribute.Version;
                Logger.Log("Loaded Mod: {1} v{2} ({0})", mod.ModId, mod.ModName, mod.Version);
                Mods.Add(mod);
                mod.Load();
            }

            foreach (var mod in Mods)
            {
                mod.PostLoad();
            }

            ModHooks.CreateHooks();
        }

        static void FindMods(Assembly assembly)
        {
            try
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(IkenFelineModAttribute), true).Length > 0)
                    {
                        modTypes.Add(type);
                    }
                }
            } catch (Exception e) {
                Logger.Log(e.StackTrace);
            }
        }

        private static void CollectPatchSets()
        {
            patchSets = new HashSet<string>();

            var directories = Directory.GetDirectories(Paths.ModDirectory);

            foreach (var dir in directories)
            {
                patchSets.Add(dir);

                var baseName = Path.GetFileNameWithoutExtension(dir);
                var basePath = Path.Combine(dir, baseName + ".dll");
                if (File.Exists(basePath))
                {
                    var assembly = Assembly.LoadFrom(basePath);
                    FindMods(assembly);
                }
            }
        }

        private static void CollectPatches()
        {
            patches = new HashSet<string>();
            assemblies = new HashSet<string>();
            var files = Directory.GetFiles(Paths.ModDirectory, "*.mm.dll", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                patches.Add(file);
                GetAssembliesFromFile(file);
            }
        }

        private static void GetAssembliesFromFile(string file)
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
                            assemblies.Add(assRef.Name + ".dll");
                        }
                        
                    }
                }
            } catch { }
        }

        public static void ApplyPatch(AssemblyDefinition ass, string assName)
        {
            using (var monoModder = new RuntimeMonoModder(ass, assName))
            {
                monoModder.LogVerboseEnabled = false;
                monoModder.DependencyDirs.AddRange(ResolveDirectories);

                var resolver = (BaseAssemblyResolver)monoModder.AssemblyResolver;
                var moduleResolver = (BaseAssemblyResolver)monoModder.Module.AssemblyResolver;

                foreach (var dir in ResolveDirectories)
                {
                    resolver.AddSearchDirectory(dir);
                }

                resolver.ResolveFailure += ResolverOnResolveFailure;
                // Add our dependency resolver to the assembly resolver of the module we are patching
                moduleResolver.ResolveFailure += ResolverOnResolveFailure;

                monoModder.PerformPatches(Paths.ModDirectory);

                // Then remove our resolver after we are done patching to not interfere with other patchers
                moduleResolver.ResolveFailure -= ResolverOnResolveFailure;
            }
        }

        // yeah.... this is really bad
        public static void SwapsieDoodles(string assemblyName)
        {
            var originalPath = Path.Combine(Paths.ExecutableDirectory, $"{assemblyName}");
            var outputPath = Path.Combine(Paths.ExecutableDirectory, $"_{assemblyName}");
            var tempPath = Path.Combine(Paths.ExecutableDirectory, $"__{assemblyName}");
            assemblyName = assemblyName.Replace(".dll", "");

            File.Move(originalPath, tempPath);
            File.Move(outputPath, originalPath);

            var assembly = Assembly.Load(assemblyName);

            File.Move(originalPath, outputPath);
            File.Move(tempPath, originalPath);

            FindMods(assembly);
        }

        public static void ApplyPatches()
        {
            foreach (var assName in assemblies) {
                using (var ass = AssemblyDefinition.ReadAssembly(assName))
                {
                    ApplyPatch(ass, assName);
                }
                SwapsieDoodles(assName);
            }
        }

        private static List<string> FindPotentialResolveFiles(string name)
        {
            var potentialDirectories = new List<string>();
            var potentialFiles = new List<string>();

            if (name.IndexOf(",") > -1)
            {
                name = name.Substring(0, name.IndexOf(","));
            }

            foreach (var directory in ResolveDirectories)
            {
                potentialDirectories.AddRange(Directory.GetDirectories(directory, "*", SearchOption.AllDirectories));

                potentialFiles.AddRange(potentialDirectories.Select(x => Path.Combine(x, $"{name}.dll"))
                                                            .Concat(potentialDirectories.Select(
                                                                        x => Path.Combine(x, $"{name}.exe"))));
            }
            return potentialFiles;
        }

        private static Assembly AppDomainResolveHandler(object sender, ResolveEventArgs args)
        {
            var potentialFiles = FindPotentialResolveFiles(args.Name);
            foreach (string path in potentialFiles)
            {
                if (!File.Exists(path))
                    continue;

                var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters(ReadingMode.Deferred));

                if (assembly.Name.FullName == args.Name)
                {
                    assembly.Dispose();
                    var loadedAssembly = Assembly.LoadFrom(path);
                    FindMods(loadedAssembly);
                    return loadedAssembly;
                }

                assembly.Dispose();
            }

            return null;
        }

        private static AssemblyDefinition ResolverOnResolveFailure(object sender, AssemblyNameReference reference)
        {
            var potentialFiles = FindPotentialResolveFiles(reference.Name);
            foreach (string path in potentialFiles)
            {
                if (!File.Exists(path))
                    continue;

                var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters(ReadingMode.Deferred));

                if (assembly.Name.Name == reference.Name)
                    return assembly;

                assembly.Dispose();
            }

            return null;
        }

    }
}
