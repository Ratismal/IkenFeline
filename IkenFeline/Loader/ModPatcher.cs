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
        private static List<string> assemblies;
        private static HashSet<string> patches;
        private static List<Type> modTypes;
        public static List<IkenFelineMod> Mods;

        public static string[] ResolveDirectories { get; set; }

        private static readonly HashSet<string> UnpatchableAssemblies =
            new HashSet<string>(StringComparer.CurrentCultureIgnoreCase) { "mscorlib" };

        public static List<string> Patch(Dictionary<string, HashSet<string>> patchMap)
        {
            patchSets = new HashSet<string>();
            assemblies = patchMap.Keys.ToList();
            foreach (var set in patchMap.Values)
            {
                foreach (var patch in set)
                {
                    patchSets.Add(Path.GetDirectoryName(patch));
                }
            }

            ResolveDirectories = new string[] {
                Paths.ExecutableDirectory,
                Paths.ModDirectory
            };

            BackupFiles();

            ResolveDirectories = ResolveDirectories.Concat(patchSets).ToArray();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AppDomainResolveHandler);

            ApplyPatches(patchMap);

            ReplaceFiles();

            return assemblies;
        }

        static void BackupFiles()
        {
            foreach (var assembly in assemblies) {
                var ass = Path.GetFileNameWithoutExtension(assembly);
                var ext = Path.GetExtension(assembly);
                var originalPath = Path.Combine(Paths.ExecutableDirectory, $"{ass}");
                var outputPath = Path.Combine(Paths.ExecutableDirectory, $"{ass}.Modded");
                var tempPath = Path.Combine(Paths.ExecutableDirectory, $"{ass}.Original");

                // backup files
                if (!File.Exists(tempPath + ext))
                {
                    File.Copy(originalPath + ext, tempPath + ext);
                    if (File.Exists(originalPath + ".pdb"))
                    {
                        File.Copy(originalPath + ".pdb", tempPath + ".pdb");
                    }
                }
                else if (File.Exists(tempPath + ext))
                {
                    File.Delete(originalPath + ext);
                    File.Delete(originalPath + ".pdb");

                    File.Copy(tempPath + ext, originalPath + ext);
                    if (File.Exists(tempPath + ".pdb"))
                    {
                        File.Copy(tempPath + ".pdb", originalPath + ".pdb");
                    }
                }
            }
        }

        static void ReplaceFiles()
        {
            foreach (var assembly in assemblies)
            {
                var ass = Path.GetFileNameWithoutExtension(assembly);
                var ext = Path.GetExtension(assembly);
                var originalPath = Path.Combine(Paths.ExecutableDirectory, $"{ass}");
                var outputPath = Path.Combine(Paths.ExecutableDirectory, $"{ass}.Modded");
                var tempPath = Path.Combine(Paths.ExecutableDirectory, $"{ass}.Original");

                File.Delete(originalPath + ext);
                File.Delete(originalPath + ".pdb");

                File.Copy(outputPath + ext, originalPath + ext);
                File.Copy(outputPath + ".pdb", originalPath + ".pdb");
            }
        }

        public static void ApplyPatch(AssemblyDefinition ass, string assName, HashSet<string> patches)
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

                monoModder.PerformPatches(patches);

                // Then remove our resolver after we are done patching to not interfere with other patchers
                moduleResolver.ResolveFailure -= ResolverOnResolveFailure;
            }
        }

        public static void ApplyPatches(Dictionary<string, HashSet<string>> patchMap)
        {
            foreach (var assName in patchMap.Keys) {
                using (var ass = AssemblyDefinition.ReadAssembly(assName))
                {
                    ApplyPatch(ass, assName, patchMap[assName]);
                }
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
