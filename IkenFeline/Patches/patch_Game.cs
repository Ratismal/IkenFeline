#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleWitch
{
    public class patch_Game : Game
    {
        public extern void orig_ctor();
        [MonoModConstructor]
        public void ctor()
        {
           

            orig_ctor();
            // LoggingManager.Dispose();
        }
    }
}
