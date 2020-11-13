using IkenFeline.Loader;
using System;
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
                foreach (IkenFelineMod mod in ModPatcher.Mods)
                {
                    mod.PreGameLoad();
                }

                orig(game);

                foreach (IkenFelineMod mod in ModPatcher.Mods)
                {
                    mod.PostGameLoad();
                }
            };
        }
    }
}
