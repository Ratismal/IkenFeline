#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleWitch
{
    [MonoModPatch("global::LittleWitch.BattleLayouts")]
    public static class patch_BattleLayouts
    {
        public extern static void orig_Init();
        public static void Init()
        {
            // call original function
            // orig_Init();

            TestMod.TestMod.Instance.Logger.Log("Adding new battle layouts");

            // add new layouts
            BattleLayout.Create("some_name").Add<EnemyBookworm>(3);
            // etc.
        }
    }
}
