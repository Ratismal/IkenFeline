using IkenFeline;
using IkenFeline.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleWitch.TestMod.mm
{
    [IkenFelineMod("me.stupidcat.testmod", "TestMod", "0.1")]
    public class TestMod : IkenFelineMod
    {
        public override void Load()
        {
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
