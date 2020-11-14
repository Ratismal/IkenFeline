using GameEngine;
using LittleWitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFelineLauncher
{
    class Steamless
    {
        public static void Start()
        {
            Platform.Init();
            using (Game game = new Game())
            {
                game.RunGame();
            }
        }
    }
}
