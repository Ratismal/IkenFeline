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
        private static RuntimeMonoModder modder;
        private static HashSet<string> patchSets;
        private static HashSet<string> assemblies;
        private static HashSet<string> patches;
        private static AppDomain domain;

        public static string[] ResolveDirectories { get; set; }
        

        private static readonly HashSet<string> UnpatchableAssemblies =
            new HashSet<string>(StringComparer.CurrentCultureIgnoreCase) { "mscorlib" };

        public static void Initialize()
        {
            AppDomain.CurrentDomain.SetupInformation.DisallowApplicationBaseProbing = true;
            AppDomain.CurrentDomain.AssemblyResolve += InterceptResolveEventHandler;
            Logger.Log("Checking for patches...");
            ResolveDirectories = new string[] {
                Paths.ExecutableDirectory,
            };

            AppDomainSetup domaininfo = new AppDomainSetup();
            domaininfo.ApplicationBase = Paths.ExecutableDirectory;
            Evidence adevidence = AppDomain.CurrentDomain.Evidence;
            domain = AppDomain.CreateDomain("PatchedGame", adevidence, domaininfo);

            // AppDomain.Unload(domain);

            CollectPatchSets();
            CollectPatches();

            ApplyPatches();


            // domain.ExecuteAssembly("MONOMODDED_IkenfellWin.exe");
        }

        private static Assembly InterceptResolveEventHandler(object sender, ResolveEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("wtf");
            System.Diagnostics.Debug.WriteLine(args.Name);
            return null;
        }

        private static void CollectPatchSets()
        {
            patchSets = new HashSet<string>();

            var directories = Directory.GetDirectories(Paths.ModDirectory);

            foreach (var dir in directories)
            {
                patchSets.Add(dir);
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
                monoModder.LogVerboseEnabled = true;
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

            Assembly.Load(assemblyName);

            File.Move(originalPath, outputPath);
            File.Move(tempPath, originalPath);
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

        private static AssemblyDefinition ResolverOnResolveFailure(object sender, AssemblyNameReference reference)
        {
            foreach (var directory in ResolveDirectories)
            {
                var potentialDirectories = new List<string> { directory };

                potentialDirectories.AddRange(Directory.GetDirectories(directory, "*", SearchOption.AllDirectories));

                var potentialFiles = potentialDirectories.Select(x => Path.Combine(x, $"{reference.Name}.dll"))
                                                         .Concat(potentialDirectories.Select(
                                                                     x => Path.Combine(x, $"{reference.Name}.exe")));

                foreach (string path in potentialFiles)
                {
                    if (!File.Exists(path))
                        continue;

                    var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters(ReadingMode.Deferred));

                    if (assembly.Name.Name == reference.Name)
                        return assembly;

                    assembly.Dispose();
                }
            }

            return null;
        }

    }
}
