using GameEngine;
using LittleWitch;
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
            Console.WriteLine("wow!");
            Console.Read();
            Platform.Init();
            using (Game game = new Game())
            {
                game.RunGame();
            }
        }
    }
}
