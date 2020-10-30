#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using GameEngine;
using IkenFeline;
using MonoMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleWitch
{
    class patch_TitleScreen : TitleScreen
    {
        public extern void orig_ctor();

        [MonoModConstructor]
        public void ctor()
        {
            // disable splash screens so we can get right into the game
            DoSplashScreens = false;
            DoPressAToStart = false;
            DoIntroAnimation = false;

            orig_ctor();
        }

        private extern IEnumerator orig_Selecting();
        private IEnumerator Selecting()
        {
            // string a = Directory.GetCurrentDirectory().Replace('\\', '/');
            // string a = Paths.WorkingDirectory.Replace('\\', '/');
            string a = "wow!";
            TextRenderer control = base.AddComponent<TextRenderer>(new TextRenderer(Game.MedFont, a, TextAlign.Right, Color.White));
            control.Offset = new Vector(236f, 5f);
            control.Shadow = new ColorF?(ColorF.Black);
            control.Tags = GameTags.UI;

            yield return orig_Selecting();
            yield break;
        }

        private extern void orig_CreateMenuItems();
        private void CreateMenuItems()
        {
            orig_CreateMenuItems();

            MenuItemLabel label = this.menuItems.Add<MenuItemLabel>(new MenuItemLabel("Mods", new Vector(100f, 117f)));
            label.OnPress += delegate ()
            {
                Master.Instance.GotoRoom(new Cell3D(4, 15, 0), true);
            };
        }

        private MenuController menuItems;
    }
}
