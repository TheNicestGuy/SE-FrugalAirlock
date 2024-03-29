﻿using Sandbox.Game.EntityComponents;
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
        // WARNING: Do not set the main loop to Update1 (every tick). This causes weird bugs.
        private const UpdateFrequency MAIN_LOOP_FREQUENCY = UpdateFrequency.Update10;
        private const UpdateType MAIN_LOOP_TYPE = UpdateType.Update10;

        #endregion Program / Constants

        public Program()
        {
            this.Runtime.UpdateFrequency = MAIN_LOOP_FREQUENCY;

            // Map known commands to methods
            this._knownCommands["rediscover"] = RediscoverCommand;
            this._knownCommands["forcemode"] = ForceModeCommand;
            this._knownCommands["queuemode"] = QueueModeCommand;

            // Perform initial discovery
            this.RediscoverCommand(true);

            // Load state from storage
            if (this._theIniParser.TryParse(this.Storage))
            {
                List<string> airlockNames = new List<string>();
                this._theIniParser.GetSections(airlockNames);
                foreach (string thisAirlockName in airlockNames)
                {
                    if (this._allAirlocks.ContainsKey(thisAirlockName))
                    {
                        //List<MyIniKey> thisAirlockKeys = new List<MyIniKey>();
                        //this._theIniParser.GetKeys(thisAirlockName, thisAirlockKeys);
                        this._allAirlocks[thisAirlockName].Deserialize(this._theIniParser);
                        this._allAirlocks[thisAirlockName].Update();
                    }
                    else
                    {
                        throw new Exception(
                            $"Saved state included a section called '{thisAirlockName}', but no airlock by this name exists."
                        );
                    }
                } // foreach thisAirlockName
            } // if could parse Storage
            this._theIniParser.Clear();
        } // Program() constructor

        public void Save()
        {
            this._theIniParser.Clear();
            foreach (Airlock thisAirlock in this._allAirlocks.Values)
            {
                thisAirlock.Serialize(this._theIniParser);
            }
            this.Storage = this._theIniParser.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // This looks like an automatic call for the main loop.
            if ((updateSource & (MAIN_LOOP_TYPE)) != 0)
            {
                foreach (Airlock thisAirlock in this._allAirlocks.Values)
                {
                    if (thisAirlock.IsComplete()) thisAirlock.Update();
                }
            }

            // This is NOT an automatic call. Run it through the CLI.
            if ((updateSource & (
                UpdateType.Update1 | UpdateType.Update10 | UpdateType.Update100
            )) == 0)
            {
                if (this._theCommandParser.TryParse(argument))
                {
                    Action commandAction;
                    if (this._theCommandParser.ArgumentCount > 0)
                    {
                        string commandName = this._theCommandParser.Argument(0);
                        if (null != commandName)
                        {
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
                else
                {
                    Echo("Program was run with no argument, and not by itself. Doing nothing.");
                }
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
                this._allAirlocks[airlockName].SetModeNow(Airlock.Mode.OpenToHabitat);
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
                foreach (Airlock thisAirlock in this._allAirlocks.Values)
                {
                    thisAirlock.SetModeNow(Airlock.Mode.OpenToHabitat);
                }
            }

            ReportStatus();
        } // RediscoverCommand(bool forceAll)

        private void ForceModeCommand()
        {
            if (this._theCommandParser.ArgumentCount < 3)
            {
                throw new Exception(
                    "Invalid syntax for the forcemode command."
                    + $"\nInput: {this._theCommandParser.ToString()}"
                    + "\nUsage: forcemode mode airlockname"
                );
            }

            Airlock.Mode targetMode;
            if (!Enum.TryParse<Airlock.Mode>(this._theCommandParser.Argument(1), true, out targetMode))
            {
                throw new Exception(
                    "Invalid syntax for the forcemode command."
                    + $"\n'{this._theCommandParser.Argument(1)}' is not a valid airlock mode."
                );
            }

            Airlock theAirlock;
            if (
                !this._allAirlocks.TryGetValue(this._theCommandParser.Argument(2), out theAirlock)
                ||
                null == theAirlock
            )
            {
                throw new Exception(
                    "Cannot complete the forcemode command."
                    + $"\nNo airlock named '{this._theCommandParser.Argument(2)}' was found. Try a rediscover command?"
                );
            }

            if (!theAirlock.IsComplete())
            {
                throw new Exception(
                    "Cannot complete the forcemode command."
                    + $"\nThe airlock named '{this._theCommandParser.Argument(2)}' is not completely functional."
                    + $"\nDetails: {theAirlock.BadConfigReport}"
                );
            }

            theAirlock.SetModeNow(targetMode);

        } //ForceModeCommand()

        private void QueueModeCommand()
        {
            if (this._theCommandParser.ArgumentCount < 3)
            {
                throw new Exception(
                    "Invalid syntax for the queuemode command."
                    + $"\nInput: {this._theCommandParser.ToString()}"
                    + "\nUsage: queuemode mode airlockname [gracedelay] [-cutinline]"
                );
            }
            
            Airlock.Mode newMode;
            if (!Enum.TryParse<Airlock.Mode>(this._theCommandParser.Argument(1), true, out newMode))
            {
                throw new Exception(
                    "Invalid syntax for the queuemode command."
                    + $"\n'{this._theCommandParser.Argument(1)}' is not a valid airlock mode."
                );
            }

            Airlock theAirlock;
            if (
                !this._allAirlocks.TryGetValue(this._theCommandParser.Argument(2), out theAirlock)
                ||
                null == theAirlock
            )
            {
                throw new Exception(
                    "Cannot complete the queuemode command."
                    + $"\nNo airlock named '{this._theCommandParser.Argument(2)}' was found. Try a rediscover command?"
                );
            }

            if (!this._theCommandParser.Switch("cutinline") && theAirlock.QueuedMode.HasValue)
            {
                // Command did not ask to cut in line, so leave the existing QueuedMode alone.
                Echo(
                    $"queuemode command did nothing. A mode is already queued for '{theAirlock.Name}', and the '-cutinline' switch was not used."
                );
                return;
            }

            double graceDelaySeconds = 0.0;
            if (
                this._theCommandParser.ArgumentCount == 4
                &&
                !Double.TryParse(this._theCommandParser.Argument(3), out graceDelaySeconds)
            )
            {
                throw new Exception(
                   "Cannot complete the queuemode command."
                   + $"\nThe gracedelay argument must be a number. '{this._theCommandParser.Argument(3)}' was given."
               );
            }

            theAirlock.QueueMode(newMode, graceDelaySeconds);
        } //QueueModeCommand()

        #endregion Command Implementations
    }
}
