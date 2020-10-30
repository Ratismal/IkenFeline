// using GameEngine;
// using LittleWitch;
using GameEngine;
using LittleWitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleWitchMain
{
    class MainProgram
    {
        public static void StartGame()
        {
            Platform.Init();
            using (Game game = new Game())
            {
                game.RunGame();
            }
        }
    }
}
