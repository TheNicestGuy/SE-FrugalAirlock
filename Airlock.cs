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
            /// cosmetic, as it may appear in-game on informational
            /// readouts.
            /// </remarks>
            public string Name { get; set; }

            #region Mandatory Blocks

            #region OuterDoors

            private IList<IMyDoor> _outerDoors;
            /// <summary>
            /// Returns references to all of this airlock's "outer doors":
            /// passable doors that can seal the airlock chamber from the
            /// vacuum.
            /// </summary>
            /// <remarks>
            /// <para>Eligible block types are the Airtight Hangar Door, the
            /// "Door" (the halves pull straight out into the walls), and the
            /// Sliding Door (halves rotate against the walls). Blast Doors are
            /// not airtight in any combination, thus not eligible. Exotic
            /// constructs that somehow manage to create a seal with
            /// piston/rotor subgrids are not recognized by this script.</para>
            /// <para>The returned collection is immutable, though the doors
            /// themselves can be manipulated by reference.</para>
            /// </remarks>
            public IEnumerable<IMyDoor> OuterDoors
            {
                get
                {
                    return new List<IMyDoor>(_outerDoors);
                }
            }

            public void AddOuterDoor(IMyDoor newDoor)
            {
                this._outerDoors.Add(newDoor);
            }

            //TODO Add methods to manipulate outer doors, like opening, closing, locking

            #endregion // OuterDoors

            #region InnerDoors

            private IList<IMyDoor> _innerDoors;
            /// <summary>
            /// Returns references to all of this airlock's "inner doors":
            /// passable doors that can seal the airlock chamber from the
            /// habitat.
            /// </summary>
            /// <remarks>
            /// <para>Eligible block types are the Airtight Hangar Door, the
            /// "Door" (the halves pull straight out into the walls), and the
            /// Sliding Door (halves rotate against the walls). Blast Doors are
            /// not airtight in any combination, thus not eligible. Exotic
            /// constructs that somehow manage to create a seal with
            /// piston/rotor subgrids are not recognized by this script.</para>
            /// <para>The returned collection is immutable, though the doors
            /// themselves can be manipulated by reference.</para>
            /// </remarks>
            public IEnumerable<IMyDoor> InnerDoors
            {
                get
                {
                    return new List<IMyDoor>(_innerDoors);
                }
            }

            public void AddInnerDoor(IMyDoor newDoor)
            {
                this._innerDoors.Add(newDoor);
            }

            //TODO Add methods to manipulate inner doors, like opening, closing, locking

            #endregion // InnerDoors

            #endregion // Mandatory Blocks

            #region Constructors

            private Airlock()
            {
                this._outerDoors = new List<IMyDoor>();
                this._innerDoors = new List<IMyDoor>();
            }

            /// <summary>
            /// Create a new instance of the Airlock class with a given name.
            /// </summary>
            /// <param name="name"></param>
            public Airlock(string name) : this()
            {
                this.Name = name;

            }

            #endregion // Constructors
        }
    }
}
