﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace Anti_Rito
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            OneTickOneSpell.Init2();
            OneTickOneSpell.Init();
            Chat.Print("Anti-Riot by Live To Rise <3");
        }
    }
}
