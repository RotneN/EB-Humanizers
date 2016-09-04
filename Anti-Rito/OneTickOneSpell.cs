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
using SharpDX.Direct3D9;
using SharpDX.Direct3D;
using System.Drawing;

namespace Anti_Rito
{
    public static class OneTickOneSpell
    {
        public static AIHeroClient Player { get{ return ObjectManager.Player; } }
        public static LastSpellCast LastSpell = new LastSpellCast();
        public static List<LastSpellCast> LastSpellsCast = new List<LastSpellCast>();
        public static int BlockedCount = 0;
        public static Menu menu, config, OneSpell;
        public static void Init2()
        {
            menu = MainMenu.AddMenu("Anti Rito", "Anti Rito");
            menu.AddGroupLabel("Anti Rito");
            menu.AddLabel("Version: " + "1.0.0.0");
            menu.AddSeparator();
            menu.AddLabel("MostlyPride");
            menu.AddSeparator();
            menu.AddLabel("+Rep If you use this :)");

            OneSpell = menu.AddSubMenu("OneSpellOneTick", "OneSpellOneTick");
            OneSpell.AddGroupLabel("OneSpellOneTick Settings");
            OneSpell.Add("Enable", new CheckBox("Enable", true));
            OneSpell.Add("Drawing", new CheckBox("Draw Block Count", true));
            //OneSpell.AddItem(new MenuItem("Recast", "Re-cast blocked spell after a delay?").SetValue(true));
        }
        public static void Init()
        {
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Drawing.OnDraw += onDrawArgs =>
                {
                    if (OneSpell["Drawing"].Cast<CheckBox>().CurrentValue) 
                    {
                        Drawing.DrawText(180, 100, System.Drawing.Color.Aqua, "Blocked" + BlockedCount + "Spells");
                        //(Drawing.Width - 180, 100, System.Drawing.Color.Lime, "Blocked " + BlockedCount + " Spells");
                    }
                };
        }
        
        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!OneSpell["Enable"].Cast<CheckBox>().CurrentValue)
                return;
            if (!sender.Owner.IsMe)
                return;
            if (!(new SpellSlot[] {SpellSlot.Q,SpellSlot.W,SpellSlot.E,SpellSlot.R,SpellSlot.Summoner1,SpellSlot.Summoner2
                ,SpellSlot.Item1,SpellSlot.Item2,SpellSlot.Item3,SpellSlot.Item4,SpellSlot.Item5,SpellSlot.Item6,SpellSlot.Trinket})
                .Contains(args.Slot))
                return;
            if (Environment.TickCount - LastSpell.CastTick < 50)
            {
                args.Process = false;
                BlockedCount += 1;
            }
            else
            {
                LastSpell = new LastSpellCast() { Slot = args.Slot, CastTick = Environment.TickCount};
            }
            if (LastSpellsCast.Any(x => x.Slot == args.Slot))
            {
                LastSpellCast spell = LastSpellsCast.FirstOrDefault(x => x.Slot == args.Slot);
                if (spell != null)
                {
                    if (Environment.TickCount - spell.CastTick <= 250 + Game.Ping)
                    {
                        args.Process = false;
                        BlockedCount += 1;
                    }
                    else
                    {
                        LastSpellsCast.RemoveAll(x => x.Slot == args.Slot);
                        LastSpellsCast.Add(new LastSpellCast() { Slot = args.Slot, CastTick = Environment.TickCount });
                    }
                }
                else
                {
                    LastSpellsCast.Add(new LastSpellCast() { Slot = args.Slot, CastTick = Environment.TickCount });
                }
            }
            else
            {
                LastSpellsCast.Add(new LastSpellCast() { Slot = args.Slot, CastTick = Environment.TickCount });
            }
        }
        public class LastSpellCast
        {
            public SpellSlot Slot = SpellSlot.Unknown;
            public int CastTick = 0;
        }
    }
}
