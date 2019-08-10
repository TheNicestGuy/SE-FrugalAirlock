using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        #region Program / Private Members
        
        /// <summary>
        /// Dictionary of all discovered Airlocks, keyed by Name.
        /// </summary>
        private Dictionary<string, Airlock> _allAirlocks;

        private MyIni _theIniParser = new MyIni();

        #endregion Program / Private Members

        #region Program / Constants

        private const string INI_SECTION_NAME = "TNGFrugalAirlock";
        private const string INI_AIRLOCK_KEY = "Airlock";
        private const string INI_ROLE_KEY = "Role";

        #endregion Program / Constants

        public Program()
        {
            List<IMyTerminalBlock> allAirlockBlocks = new List<IMyTerminalBlock>();
            this.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(
                allAirlockBlocks
                , block => MyIni.HasSection(block.CustomData, INI_SECTION_NAME)
            );
            this._allAirlocks =
                new Dictionary<string, Airlock>(Airlock.DiscoverAllAirlocks(
                    allAirlockBlocks
                    , this._theIniParser
                    , INI_SECTION_NAME
                ));
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo(
                $"Found {this._allAirlocks.Count().ToString()} airlock(s)."
            );
            foreach (Airlock a in this._allAirlocks.Values)
            {
                Echo($"{a.Name} - {(a.IsComplete() ? "valid" : a.BadConfigReport)}");
            }
        }
    }
}
