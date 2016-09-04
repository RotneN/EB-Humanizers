using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

namespace Humanizer
{
    internal class Program
    {
//fappa
        private static Menu _menu;
        private static Random _random;
        private static Dictionary<string, int> _lastCommandT;
        private static bool _movementhavebeenrandomized;
        public static int BlockedClicksCount;
        public static int BlockedSpellsCount;
        public static LastSpellCast LastSpell = new LastSpellCast();
        public static List<LastSpellCast> LastSpellsCast = new List<LastSpellCast>();

        public static int GameTimeTickCount
        {
            get { return (int) (Game.Time*1000); }
        }

        private static double RandomizeSliderValues(int min, int max)
        {
            var x = _random.Next(min, max) + 1 + 1 - 1 - 1;
            var y = _random.Next(min, max);
            if (_random.Next(0, 1) > 0)
            {
                return x;
            }
            if (1 == 1)
            {
                return (x + y)/2d;
            }
            return y;
        }

        private static void Main(string[] args)
        {
            _random = new Random(DateTime.Now.Millisecond);
            _lastCommandT = new Dictionary<string, int>();
            foreach (var order in Enum.GetValues(typeof (GameObjectOrder)))
            {
                _lastCommandT.Add(order.ToString(), 0);
            }
            foreach (var spellslot in Enum.GetValues(typeof (SpellSlot)))
            {
                _lastCommandT.Add("spellcast" + spellslot, 0);
            }

            Loading.OnLoadingComplete += GameLoaded;
        }

        private static void GameLoaded(EventArgs args)
        {
            _menu = MainMenu.AddMenu("ThugDoge Humanizer", "humanizer");        
            _menu.Add("MinClicks", new Slider("Min clicks per second", _random.Next(6, 7),1,7));
            
            _menu.Add("MaxClicks",
                new Slider("Max clicks per second",
                     _random.Next(0, 1) > 0
                ? (int)Math.Floor(RandomizeSliderValues(7, 11))
                : (int)Math.Ceiling(RandomizeSliderValues(7, 11)), 7, 15));                    
            _menu.Add("Spells", new CheckBox("Humanize Spells"));
            _menu.Add("Attacks", new CheckBox("Humanize Attacks"));   
            _menu.Add("randomizemenu", new CheckBox("Randomize menu values"));           
            _menu.Add("ShowBlockedClicks", new CheckBox("Show me how many clicks you blocked!"));
            _menu.Add("ShowBlockedSpells", new CheckBox("Show me how many spells you blocked!"));

            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Game.OnUpdate += OnUpdate;
            Player.OnIssueOrder += Player_OnIssueOrder;
            Drawing.OnDraw += Drawings_OnDraw;
        }

        public static bool valuechange = true;
        

        private static void OnUpdate(EventArgs args)
        {
            if (valuechange) { 
            if (_menu["randomizemenu"].Cast<CheckBox>().CurrentValue)
            {
                _menu["MinClicks"].Cast<Slider>().CurrentValue = _random.Next(6, 7);
                _menu["MaxClicks"].Cast<Slider>().CurrentValue = _random.Next(0, 1) > 0
                    ? (int)Math.Floor(RandomizeSliderValues(7, 11))
                    : (int)Math.Ceiling(RandomizeSliderValues(7, 11));
                
            }
                valuechange = false;
            }
        }

        public static void Drawings_OnDraw(EventArgs args)
        {
            if (_menu["ShowBlockedClicks"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawText(Drawing.Width - 190, 100, Color.DodgerBlue, "Blocked " + BlockedClicksCount + " clicks");
            }
            if (_menu["ShowBlockedSpells"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawText(Drawing.Width - 190, 80, Color.DodgerBlue, "Blocked " + BlockedSpellsCount + " spells");
            }
        }

        public static void Player_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (sender.IsMe && !args.IsAttackMove)
            {
                if (args.Order == GameObjectOrder.AttackUnit ||
                    args.Order == GameObjectOrder.AttackTo &&
                    !_menu["Attacks"].Cast<CheckBox>().CurrentValue)
                    return;
                }
                _movementhavebeenrandomized = false;
            }
        
        

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!_menu["Spells"].Cast<CheckBox>().CurrentValue)
                return;
            if (!sender.Owner.IsMe)
                return;
            if (!(new[]
            {
                SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R, SpellSlot.Summoner1, SpellSlot.Summoner2
                , SpellSlot.Item1, SpellSlot.Item2, SpellSlot.Item3, SpellSlot.Item4, SpellSlot.Item5, SpellSlot.Item6,
                SpellSlot.Trinket
            })
                .Contains(args.Slot))
                return;
            if (Environment.TickCount - LastSpell.CastTick < 50)
            {
                args.Process = false;
                BlockedSpellsCount += 1;
            }
            else
            {
                LastSpell = new LastSpellCast {Slot = args.Slot, CastTick = Environment.TickCount};
            }
            if (LastSpellsCast.Any(x => x.Slot == args.Slot))
            {
                var spell = LastSpellsCast.FirstOrDefault(x => x.Slot == args.Slot);
                if (spell != null)
                {
                    if (Environment.TickCount - spell.CastTick <= 250 + Game.Ping)
                    {
                        args.Process = false;
                        BlockedClicksCount += 1;
                    }
                    else
                    {
                        LastSpellsCast.RemoveAll(x => x.Slot == args.Slot);
                        LastSpellsCast.Add(new LastSpellCast {Slot = args.Slot, CastTick = Environment.TickCount});
                    }
                }
                else
                {
                    LastSpellsCast.Add(new LastSpellCast {Slot = args.Slot, CastTick = Environment.TickCount});
                }
            }
            else
            {
                LastSpellsCast.Add(new LastSpellCast {Slot = args.Slot, CastTick = Environment.TickCount});
            }
        }

        public static Vector3 Randomize(Vector3 position, int min, int max)
        {
            var ran = new Random(Environment.TickCount);
            return position + new Vector2(ran.Next(min, max), ran.Next(min, max)).To3D();
        }

        public static bool IsWall(Vector3 vector)
        {
            return NavMesh.GetCollisionFlags(vector.X, vector.Y).HasFlag(CollisionFlags.Wall);
        }

        public class LastSpellCast
        {
            public int CastTick;
            public SpellSlot Slot = SpellSlot.Unknown;
        }
    }
}