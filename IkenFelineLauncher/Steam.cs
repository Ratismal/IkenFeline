using GameEngine;
using LittleWitch;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkenFelineLauncher
{
    class Steam
    {
        private static readonly AppId_t APP_ID = new AppId_t(854940u);

        public static void Start()
        {
            if (SteamAPI.RestartAppIfNecessary(APP_ID))
            {
                return;
            }
            if (!SteamAPI.Init())
            {
                throw new Exception("SteamAPI.Init() failed");
            }
            bool userStatsReceived = false;
            Callback<UserStatsReceived_t>.Create(delegate
            {
                userStatsReceived = true;
            });
            if (!SteamUserStats.RequestCurrentStats())
            {
                throw new Exception("SteamUserStats.RequestCurrentStats() failed");
            }
            Achievements.OnUnlock += delegate (string name)
            {
                if (!userStatsReceived || !SteamUserStats.SetAchievement(name) || SteamUserStats.StoreStats())
                {
                    return;
                }
                throw new Exception("SteamUserStats.StoreStats() failed");
            };
            Platform.Init();
            using Game game = new Game();
            game.OnUpdate += SteamAPI.RunCallbacks;
            game.RunGame();
            SteamAPI.Shutdown();
        }
    }
}
