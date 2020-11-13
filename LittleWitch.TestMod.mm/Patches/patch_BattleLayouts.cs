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
        public new static void Init()
        {
            // call original function
            orig_Init();

            Console.WriteLine("Adding new battle layouts");

            // add new layouts
            BattleLayout.Create("some_name").Add<EnemyBookworm>(3);
            // etc.
        }
    }
}