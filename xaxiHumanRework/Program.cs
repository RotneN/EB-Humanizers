using System;
using EloBuddy;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace xaxiHumanReworked
{
    class Program
    {
        static Menu _menu, _settingsMenu;
        static Random _random;
        private static int _lastMoveT = 0;
        private static int _lastAttackT = 0;
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += gameLoadEventArgs =>
            {
                _random = new Random(Environment.TickCount - Environment.TickCount);
                _menu = MainMenu.AddMenu("xaxiHumanReworked", "xaxiHumanReworked");
                _menu.AddGroupLabel("xaxiHumanReworked Settings");
                _menu.AddSeparator();
                _menu.AddLabel("Rework By Xaxixeo");

                _settingsMenu = _menu.AddSubMenu("Settings", "settings");
                _settingsMenu.AddGroupLabel("Settings");
                _settingsMenu.AddLabel("Delays");
                _settingsMenu.Add("MinClicks", new Slider("Min clicks per second", _random.Next(5,6), 1, 6));
                _settingsMenu.Add("MaxClicks", new Slider("Max clicks per second", _random.Next(7,11), 7, 15));
                
            };
            Player.OnIssueOrder += (sender, issueOrderEventArgs) =>
            {
                if (sender.IsMe)
                {
                    if (issueOrderEventArgs.Order == GameObjectOrder.MoveTo)
                    {
                        if (Environment.TickCount - _lastMoveT <
                            _random.Next(1000 / _settingsMenu["MaxClicks"].Cast<Slider>().CurrentValue,
                                1000 / _settingsMenu["MinClicks"].Cast<Slider>().CurrentValue))
                        {
                            issueOrderEventArgs.Process = false;
                            return;
                        }
                        _lastMoveT = Environment.TickCount;
                    }
                    if (issueOrderEventArgs.Order == GameObjectOrder.AttackUnit)
                    {
                        if (Environment.TickCount - _lastAttackT <
                            _random.Next(1000 / _settingsMenu["MaxClicks"].Cast<Slider>().CurrentValue,
                                1000 / _settingsMenu["MinClicks"].Cast<Slider>().CurrentValue))
                        {
                            issueOrderEventArgs.Process = false;
                            return;
                        }
                        _lastAttackT = Environment.TickCount;
                    }
                }
            };
        }
    }
}
