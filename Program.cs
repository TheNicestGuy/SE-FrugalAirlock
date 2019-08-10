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
            this._allAirlocks = new Dictionary<string, Airlock>(StringComparer.CurrentCultureIgnoreCase);

            // Find all blocks with a [TNGFrugalAirlock] section
            List<IMyTerminalBlock> allAirlockBlocks = new List<IMyTerminalBlock>();
            this.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(
                allAirlockBlocks
                , block => MyIni.HasSection(block.CustomData, INI_SECTION_NAME)
            );

            // Iterate through them and parse them into airlocks and roles
            MyIniParseResult parseResult;
            string airlockName;
            Airlock thisAirlock;
            string roleName;
            foreach (IMyTerminalBlock thisBlock in allAirlockBlocks)
            {
                if (!this._theIniParser.TryParse(thisBlock.CustomData, INI_SECTION_NAME, out parseResult))
                {
                    throw new Exception(
                        $"Failed to parse airlock configuration data."
                        + $"\nBlock: {thisBlock.CustomName}"
                        + $"\nParse result: {parseResult.ToString()}"
                    );
                }

                // Parse the airlock name and get a new or existing reference to the airlock
                airlockName = this._theIniParser.Get(INI_SECTION_NAME, INI_AIRLOCK_KEY).ToString();
                if (string.IsNullOrEmpty(airlockName))
                {
                    throw new Exception(
                        $"Failed to parse airlock configuration data because no airlock name was given."
                        + $"\nBlock: {thisBlock.CustomName}"
                    );
                }

                if (this._allAirlocks.ContainsKey(airlockName))
                {
                    thisAirlock = this._allAirlocks[airlockName];
                }
                else
                {
                    thisAirlock = new Airlock(airlockName);
                    this._allAirlocks.Add(airlockName, thisAirlock);
                }

                roleName = this._theIniParser.Get(INI_SECTION_NAME, INI_ROLE_KEY).ToString();
                if (string.IsNullOrEmpty(roleName))
                {
                    throw new Exception(
                        $"Failed to parse airlock configuration data because no role was given."
                        + $"\nBlock: {thisBlock.CustomName}"
                    );
                }

                bool wrongTypeForRole = false;
                switch (roleName)
                {
                    case Airlock.ROLE_NAME_OUTERDOOR:
                        if (!(thisBlock is IMyDoor))
                        {
                            wrongTypeForRole = true;
                            break;
                        }
                        thisAirlock.AddOuterDoor(thisBlock as IMyDoor);
                        break;
                    case Airlock.ROLE_NAME_INNERDOOR:
                        if (!(thisBlock is IMyDoor))
                        {
                            wrongTypeForRole = true;
                            break;
                        }
                        thisAirlock.AddInnerDoor(thisBlock as IMyDoor);
                        break;
                    case Airlock.ROLE_NAME_FILLVENT:
                        if (!(thisBlock is IMyAirVent))
                        {
                            wrongTypeForRole = true;
                            break;
                        }
                        thisAirlock.AddFillVent(thisBlock as IMyAirVent);
                        break;
                    case Airlock.ROLE_NAME_DRAINVENT:
                        if (!(thisBlock is IMyAirVent))
                        {
                            wrongTypeForRole = true;
                            break;
                        }
                        thisAirlock.AddDrainVent(thisBlock as IMyAirVent);
                        break;
                    case Airlock.ROLE_NAME_DRAINTANK:
                        if (!(thisBlock is IMyGasTank))
                        {
                            wrongTypeForRole = true;
                            break;
                        }
                        thisAirlock.AddDrainTank(thisBlock as IMyGasTank);
                        break;
                    case Airlock.ROLE_NAME_HABBAROMETER:
                        if (!(thisBlock is IMyAirVent))
                        {
                            wrongTypeForRole = true;
                            break;
                        }
                        thisAirlock.AddHabitatBarometer(thisBlock as IMyAirVent);
                        break;
                    case Airlock.ROLE_NAME_VACBAROMETER:
                        if (!(thisBlock is IMyAirVent))
                        {
                            wrongTypeForRole = true;
                            break;
                        }
                        thisAirlock.AddVacuumBarometer(thisBlock as IMyAirVent);
                        break;
                    default:
                        throw new Exception(
                            $"Failed to parse airlock configuration data because a block's role was not recognized."
                            + $"\nBlock: {thisBlock.CustomName}"
                            + $"\nRole: {roleName}"
                        );
                } // switch (roleName)

                if (wrongTypeForRole)
                {
                    throw new Exception(
                        $"Failed to parse airlock configuration data because a block is the wrong type for its role."
                        + $"\nBlock: {thisBlock.CustomName}"
                        + $"\nRole: {roleName}"
                    );
                }

            } // foreach thisBlock
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // report on airlocks and blocks found
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
