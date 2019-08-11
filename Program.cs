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
        private MyCommandLine _theCommandParser = new MyCommandLine();
        private readonly IDictionary<string, Action> _knownCommands
            = new Dictionary<string, Action>(StringComparer.InvariantCultureIgnoreCase);

        #endregion Program / Private Members

        #region Program / Constants

        private const string INI_SECTION_NAME = "TNGFrugalAirlock";
        private const string INI_AIRLOCK_KEY = "Airlock";
        private const string INI_ROLE_KEY = "Role";

        #endregion Program / Constants

        public Program()
        {
            // Map known commands to methods
            this._knownCommands["rediscover"] = RediscoverCommand;

            // Perform initial discovery
            this.RediscoverCommand(true);
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            bool defaultRun = true;

            if (this._theCommandParser.TryParse(argument))
            {
                Action commandAction;
                if (this._theCommandParser.ArgumentCount > 0)
                {
                    string commandName = this._theCommandParser.Argument(0);
                    if (null != commandName)
                    {
                        defaultRun = false;
                        if (this._knownCommands.TryGetValue(commandName, out commandAction))
                        {
                            commandAction();
                        }
                        else
                        {
                            throw new Exception(
                                $"Command '{commandName}' does not exist."
                            );
                        }
                    } // check and map command name
                } // if arguments existed
            } // if could parse command line

            if (defaultRun) {
                ReportStatus();
            }
        } // Main()

        private void ReportStatus()
        {
            Echo(
                $"Found {this._allAirlocks.Count.ToString()} airlock(s)."
            );
            foreach (Airlock a in this._allAirlocks.Values)
            {
                Echo($"{a.Name} - {(a.IsComplete() ? "valid" : a.BadConfigReport)}");
            }
        }

        #region Command Implementations

        private void RediscoverCommand() { this.RediscoverCommand(false); }
        private void RediscoverCommand(bool forceAll)
        {
            List<IMyTerminalBlock> allAirlockBlocks = new List<IMyTerminalBlock>();
            this.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(
                allAirlockBlocks
                , block => MyIni.HasSection(block.CustomData, INI_SECTION_NAME)
            );

            // The rediscover command takes one optional argument that specifies
            // a single airlock to rediscover. forceAll overrides this and
            // rediscovers all regardless.
            if (this._theCommandParser.ArgumentCount > 1 && !forceAll)
            {
                // Rediscover one airlock
                string airlockName = this._theCommandParser.Argument(1);
                IDictionary<string, Airlock> tempAirlocks =
                    Airlock.DiscoverAllAirlocks(
                        allAirlockBlocks
                        , this._theIniParser
                        , INI_SECTION_NAME
                    )
                ;
                this._allAirlocks.Remove(airlockName);
                if (tempAirlocks.ContainsKey(airlockName))
                {
                    this._allAirlocks[airlockName] = tempAirlocks[airlockName];
                }
            }
            else
            {
                // Rediscover all airlocks
                this._allAirlocks =
                    new Dictionary<string, Airlock>(Airlock.DiscoverAllAirlocks(
                        allAirlockBlocks
                        , this._theIniParser
                        , INI_SECTION_NAME
                    ));
            }

            ReportStatus();
        } // RediscoverCommand(bool forceAll)

        #endregion Command Implemenations
    }
}
