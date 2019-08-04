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
    partial class Program
    {
        /// <summary>
        /// Represents a single airlock assembly and all of its component blocks
        /// and configuration properties.
        /// </summary>
        public class Airlock
        {
            /// <summary>
            /// Name of the airlock
            /// </summary>
            /// <remarks>
            /// This is both functional, as it appears in the configuration data
            /// to associate separate blocks into a unified airlock, and
            /// user-friendly, as it may appear in-game on informational
            /// readouts.
            /// </remarks>
            public string Name { get; set; }
        }
    }
}
