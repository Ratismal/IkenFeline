using GameEngine;
using IkenFeline.Loader;
using LittleWitch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFeline.Hooks
{
    class ModHooks
    {
        public static void CreateHooks()
        {
            On.LittleWitch.Game.ctor += (orig, game) =>
            {
                foreach (IkenFelineMod mod in ModLoader.Mods.Values)
                {
                    mod.PreGameLoad();
                }

                orig(game);

                foreach (IkenFelineMod mod in ModLoader.Mods.Values)
                {
                    mod.PostGameLoad();
                }
            };

            On.LittleWitch.TitleScreen.ctor += AddTitleScreenVersion;
        }

        static void AddTitleScreenVersion(On.LittleWitch.TitleScreen.orig_ctor orig, TitleScreen titleScreen)
        {
            orig(titleScreen);

            string versionString = $"(IkenFeline v{IkenFeline.Version})";
            TextRenderer version = titleScreen.AddComponent<TextRenderer>(new TextRenderer(Game.SmallFont, versionString, TextAlign.Left, 1497472255u));
            version.Offset = new Vector(30f, 6f);
            version.SetShadow(ColorF.Black);
            version.Tags = GameTags.UI;
            version.Depth = -100;

            var count = ModLoader.ModList.Count;
            string modsString = $"{count} Mod{(count == 1 ? "" : "s")} Loaded";
            TextRenderer loadedMods = titleScreen.AddComponent<TextRenderer>(new TextRenderer(Game.SmallFont, modsString, TextAlign.Left, 1497472255u));
            loadedMods.Offset = new Vector(1f, 15f);
            loadedMods.SetShadow(ColorF.Black);
            loadedMods.Tags = GameTags.UI;
            loadedMods.Depth = -100;
        }
    }
}
