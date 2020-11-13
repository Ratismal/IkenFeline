using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steamworks
{
    [MonoModPatch("global::Steamworks.SteamAPI")]
    class patch_SteamAPI
    {
        public static bool RestartAppIfNecessary(AppId_t unOwnAppID)
        {
            return false;
        }
    }
}
