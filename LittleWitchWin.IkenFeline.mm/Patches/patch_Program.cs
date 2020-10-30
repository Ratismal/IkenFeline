using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleWitchWin
{
    [MonoModPatch("global::LittleWitchWin.Program")]
    class patch_Program
    {
        public static void Main(string[] args)
        {

            if (AppDomain.CurrentDomain.FriendlyName != "PatchedGame")
            {
                IkenFeline.IkenFeline.Initialize();
                IkenFeline.IkenFeline.InitiatePatching();
            }
            else
            {
            }

            LittleWitchMain.MainProgram.StartGame();

        }
    }
}
