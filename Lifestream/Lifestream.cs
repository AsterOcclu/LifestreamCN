﻿using Dalamud.Game;
using ECommons.Automation;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.MathHelpers;
using ECommons.SimpleGui;
using Lumina.Excel.GeneratedSheets;

namespace Lifestream
{
    public class Lifestream : IDalamudPlugin
    {
        public string Name => "Lifestream";
        internal static Lifestream P;
        internal Config Config;
        internal TaskManager TaskManager;
        internal DataStore DataStore;

        internal TinyAetheryte? ActiveAetheryte = null;

        internal uint Territory => Svc.ClientState.TerritoryType;

        public Lifestream(DalamudPluginInterface pluginInterface)
        {
            P = this;
            ECommonsMain.Init(pluginInterface, this);
            new TickScheduler(delegate
            {
                Config = EzConfig.Init<Config>();
                EzConfigGui.Init(UI.Draw);
                EzConfigGui.WindowSystem.AddWindow(new Overlay());
                EzCmd.Add("/lifestream", EzConfigGui.Open);
                TaskManager = new()
                {
                };
                DataStore = new();
                ProperOnLogin.Register(delegate
                {
                    DataStore.BuildWorlds();
                }, true);
                Svc.Framework.Update += Framework_Update;
            });
        }

        private void Framework_Update(Framework framework)
        {
            if(Svc.ClientState.LocalPlayer != null && DataStore.Territories.Contains(Svc.ClientState.TerritoryType))
            {
                UpdateActiveAetheryte();
            }
            else
            {
                ActiveAetheryte = null;
            }
        }

        public void Dispose()
        {
            Svc.Framework.Update -= Framework_Update;
            ECommonsMain.Dispose();
            P = null;
        }

        void UpdateActiveAetheryte()
        {
            var a = Util.GetValidAetheryte();
            if (a != null)
            {
                var pos2 = a.Position.ToVector2();
                foreach (var x in DataStore.Aetherytes)
                {
                    if (x.Key.TerritoryType == Svc.ClientState.TerritoryType && Vector2.Distance(x.Key.Position, pos2) < 10)
                    {
                        ActiveAetheryte = x.Key;
                        return;
                    }
                    foreach (var l in x.Value)
                    {
                        if (l.TerritoryType == Svc.ClientState.TerritoryType && Vector2.Distance(l.Position, pos2) < 10)
                        {
                            ActiveAetheryte = l;
                            return;
                        }
                    }
                }
            }
            else
            {
                ActiveAetheryte = null;
            }
        }
    }
}