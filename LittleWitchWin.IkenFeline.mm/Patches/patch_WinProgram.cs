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
        public extern static void orig_Main(string[] args);
        public static void Main(string[] args)
        {
            IkenFeline.IkenFeline.Initialize();
            IkenFeline.IkenFeline.InitiatePatching();

            try
            {
                // attempt running through original methods
                orig_Main(args);
            } catch
            {
                // could not launch, so perform launch manually
                LittleWitchMain.MainProgram.StartGame();
            }
        }
    }
}
