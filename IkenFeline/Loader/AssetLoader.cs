using GameEngine;
using IkenFeline.LogManager;
using IkenFeline.Mod;
using LittleWitch;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline.Loader
{
    public sealed class AssetLoader
    {
        static JsonArray maps;
        public static void LoadAssets()
        {
            maps = new JsonArray();
            foreach (var manifest in ModLoader.ModList.Values)
            {
                var assetPath = Path.Combine(manifest.Path, "Content");
                if (!Directory.Exists(assetPath))
                {
                    continue;
                }

                var dataPath = Path.Combine(assetPath, "Data");
                if (Directory.Exists(dataPath))
                {
                    FindMaps(manifest, dataPath);
                }

            }

            On.LittleWitch.Game.ReloadScripts += LoadScripts;
            IL.LittleWitch.Game.LoadMap += LoadMaps;
        }

        static void FindMaps(ModManifest manifest, string directory)
        {
            var path = Path.Combine(directory, "map.json");
            if (File.Exists(path))
            {
                JsonArray jsonArray = Json.LoadArrayFile(path);

                foreach (var value in jsonArray.GetValues<JsonObject>())
                {
                    maps.Add(value);
                }
            }
        }

        static void LoadMaps(ILContext il)
        {
            var c = new ILCursor(il);
            var reference = c.AddReference<JsonArray>(maps);
            var returnLabel = c.DefineLabel();
            var skipLabel = c.DefineLabel();

            // Set V_4 to 0
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Stloc, 4);

            // Go right after jsonArray gets set to V_0
            c.GotoNext(MoveType.After, (inst) => inst.OpCode == OpCodes.Stloc_0);
            c.MarkLabel(returnLabel);

            // Need to go after the Ldstr because for some reason going before errors
            //  - likely to do with some sort of hidden finally { } statement that cecil can't see? shrug
            c.GotoNext(MoveType.After, (inst) => inst.OpCode == OpCodes.Ldstr && (string)inst.Operand == "Room count: ");

            // Pop off the Ldstr, as we don't need it
            c.Emit(OpCodes.Pop);
            // Check if V_4 is truthy, if so skip to end
            c.Emit(OpCodes.Ldloc, 4);
            c.Emit(OpCodes.Brtrue_S, skipLabel);
            // V_4 was falsey, so this is our first loop. Set jsonArray to our content, set V_4 to 1, and return to right after jsonArray gets initially set
            c.EmitGetReference<JsonArray>(reference);
            c.Emit(OpCodes.Stloc_0);
            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit(OpCodes.Stloc, 4);
            c.Emit(OpCodes.Br_S, returnLabel);

            c.MarkLabel(skipLabel);
            c.Emit(OpCodes.Ldstr, "Room count: ");
        }

        static void LoadScripts(On.LittleWitch.Game.orig_ReloadScripts orig, Game game)
        {
            orig(game);

            foreach (var manifest in ModLoader.ModList.Values)
            {
                var assetPath = Path.Combine(manifest.Path, "Content", "Scripts");
                if (!Directory.Exists(assetPath))
                {
                    continue;
                }

                string[] files = Directory.GetFiles(assetPath, "*.lua");
                foreach (var file in files)
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    string name = $"{manifest.ModId}:{nameWithoutExt}";
                    var script = File.ReadAllText(file);

                    Assets.AddScript(name, script);
                }

            }
        }
    }
}
