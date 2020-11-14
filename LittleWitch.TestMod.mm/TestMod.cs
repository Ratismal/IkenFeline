#define MAIN_ONLY

using IkenFeline;
using IkenFeline.Attributes;
using IkenFeline.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMod
{
    public class TestMod : IkenFelineMod
    {
        public static TestMod Instance;

        public TestMod(ModManifest manifest) : base(manifest)
        {
        }

        public override void Load()
        {
            Instance = this;
            Logger.Log("Mod loaded!");

            On.LittleWitch.AbilityFireball.InHitArea += (orig, ability, grid, unit, target, tile) =>
            {
                return true;
            };
            On.LittleWitch.AbilityFireball.InRange += (orig, ability, grid, unit, target) =>
            {
                return true;
            };
        }

        public override void PreGameLoad()
        {
            Logger.Log("PreGameLoad");
        }

        public override void PostGameLoad()
        {
            Logger.Log("PostGameLoad");
        }
    }
}
