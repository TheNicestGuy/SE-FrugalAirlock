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

            #region Constants, Enums, Statics

            public const string ROLE_NAME_OUTERDOOR = "OuterDoor";
            public const string ROLE_NAME_INNERDOOR = "InnerDoor";
            public const string ROLE_NAME_FILLVENT = "FillVent";
            public const string ROLE_NAME_DRAINVENT = "DrainVent";
            public const string ROLE_NAME_DRAINTANK = "DrainTank";
            public const string ROLE_NAME_HABBAROMETER = "HabitatBarometer";
            public const string ROLE_NAME_VACBAROMETER = "VacuumBarometer";

            #endregion Constants, Enums, Statics

            #region Basic Properties

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

            private StringBuilder _badConfigReport = new StringBuilder();
            /// <summary>
            /// Text explaining how this airlock is misconfigured.
            /// </summary>
            public string BadConfigReport
            {
                get
                {
                    return this._badConfigReport.ToString();
                }
            }

            #endregion Basic Properties

            #region Mandatory Blocks

            #region Mandatory Blocks / OuterDoors

            private readonly IList<IMyDoor> _outerDoors;
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

            /// <summary>
            /// Associates a door with this airlock as an Outer Door
            /// </summary>
            public void AddOuterDoor(IMyDoor newDoor)
            {
                if (null == newDoor) throw new ArgumentNullException("newDoor");
                this._outerDoors.Add(newDoor);
            }

            //TODO Add methods to manipulate outer doors, like opening, closing, locking

            #endregion Mandatory Blocks /  OuterDoors

            #region Mandatory Blocks / InnerDoors

            private readonly IList<IMyDoor> _innerDoors;
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

            /// <summary>
            /// Associates a door with this airlock as an Inner Door
            /// </summary>
            public void AddInnerDoor(IMyDoor newDoor)
            {
                if (null == newDoor) throw new ArgumentNullException("newDoor");
                this._innerDoors.Add(newDoor);
            }

            //TODO Add methods to manipulate inner doors, like opening, closing, locking

            #endregion Mandatory Blocks /  InnerDoors

            #region Mandatory Blocks / FillVents

            private readonly IList<IMyAirVent> _fillVents;
            /// <summary>
            /// Returns references to all of this airlock's "fill vents": Air
            /// Vents that are connected to main oxygen supplies and NOT to the
            /// drain tanks.
            /// </summary>
            /// <remarks>
            /// <para>The returned collection is immutable, though the vents
            /// themselves can be manipulated by reference.</para>
            /// </remarks>
            public IEnumerable<IMyAirVent> FillVents
            {
                get
                {
                    return new List<IMyAirVent>(_fillVents);
                }
            }

            /// <summary>
            /// Associates a vent with this airlock as a Fill Vent
            /// </summary>
            public void AddFillVent(IMyAirVent newVent)
            {
                if (null == newVent) throw new ArgumentNullException("newVent");
                this._fillVents.Add(newVent);
            }

            //TODO Add methods to manipulate fill vents, like sucking, blowing, shutting off

            #endregion Mandatory Blocks /  FillVents

            #region Mandatory Blocks / DrainVents

            private readonly IList<IMyAirVent> _drainVents;
            /// <summary>
            /// Returns references to all of this airlock's "drain vents": Air
            /// Vents that are connected to drainage tanks and NOTHING ELSE.
            /// </summary>
            /// <remarks>
            /// <para>The returned collection is immutable, though the vents
            /// themselves can be manipulated by reference.</para>
            /// </remarks>
            public IEnumerable<IMyAirVent> DrainVents
            {
                get
                {
                    return new List<IMyAirVent>(_drainVents);
                }
            }

            /// <summary>
            /// Associates a vent with this airlock as a Drain Vent
            /// </summary>
            public void AddDrainVent(IMyAirVent newVent)
            {
                if (null == newVent) throw new ArgumentNullException("newVent");
                this._drainVents.Add(newVent);
            }

            //TODO Add methods to manipulate drain vents, like sucking, blowing, shutting off

            #endregion Mandatory Blocks /  DrainVents

            #region Mandatory Blocks / DrainTanks

            private readonly IList<IMyGasTank> _drainTanks;
            /// <summary>
            /// Returns references to all of this airlock's "drain tanks": Air
            /// Vents that are connected to drainage tanks and NOTHING ELSE.
            /// </summary>
            /// <remarks>
            /// <para>The returned collection is immutable, though the vents
            /// themselves can be manipulated by reference.</para>
            /// </remarks>
            public IEnumerable<IMyGasTank> DrainTanks
            {
                get
                {
                    return new List<IMyGasTank>(_drainTanks);
                }
            }

            /// <summary>
            /// Associates an oxygen tank with this airlock as a Drain Tank
            /// </summary>
            /// <remarks>
            /// IMyGasTank objects are not necessarily oxygen tanks. If
            /// <paramref name="newTank"/> is not, an Exception will be thrown.
            /// </remarks>
            public void AddDrainTank(IMyGasTank newTank)
            {
                if (null == newTank) throw new ArgumentNullException("newTank");
                if (!newTank.IsForOxygen())
                {
                    throw new Exception(
                        "Attempted to add a non-oxygen tank as an airlock Draink Tank."
                        + $"Block: {newTank.CustomName}"
                    );
                }
                this._drainTanks.Add(newTank);
            }

            #endregion Mandatory Blocks /  DrainTanks

            #region Mandatory Blocks / HabitatBarometers

            private readonly IList<IMyAirVent> _habBarometers;
            /// <summary>
            /// Returns references to all of this airlock's "habitat
            /// barometers": Air Vents that face the habitat and are used to
            /// read the habitat's oxygen pressure.
            /// </summary>
            /// <remarks>
            /// <para>It should not matter whether these vents are dedicated
            /// to the airlock or part of the main life support system. There
            /// can be any number of vents in the collection, but the pressure
            /// will only ever be read from the first one.</para>
            /// <para>The returned collection is immutable, though the vents
            /// themselves can be manipulated by reference.</para>
            /// </remarks>
            public IEnumerable<IMyAirVent> HabitatBarometers
            {
                get
                {
                    return new List<IMyAirVent>(_habBarometers);
                }
            }

            /// <summary>
            /// Associates a vent with this airlock as a Habitat Barometer
            /// </summary>
            public void AddHabitatBarometer(IMyAirVent newVent)
            {
                if (null == newVent) throw new ArgumentNullException("newVent");
                this._habBarometers.Add(newVent);
            }

            #endregion Mandatory Blocks /  HabitatBarometers

            #region Mandatory Blocks / VacuumBarometers

            private readonly IList<IMyAirVent> _vacBarometers;
            /// <summary>
            /// Returns references to all of this airlock's "vacuum barometers":
            /// Air Vents that face the vacuum and are used to read the vacuum's
            /// oxygen pressure.
            /// </summary>
            /// <remarks>
            /// <para>It would be normal for these blocks to be detached from
            /// all oxygen supplies and mostly non-functional, as they are used
            /// ONLY for reading pressure. There can be any number of vents in
            /// the collection, but the pressure will only ever be read from the
            /// first one.</para>
            /// <para>The returned collection is immutable, though the vents
            /// themselves can be manipulated by reference.</para>
            /// </remarks>
            public IEnumerable<IMyAirVent> VacuumBarometers
            {
                get
                {
                    return new List<IMyAirVent>(_vacBarometers);
                }
            }

            /// <summary>
            /// Associates a vent with this airlock as a Vacuum Barometer
            /// </summary>
            public void AddVacuumBarometer(IMyAirVent newVent)
            {
                if (null == newVent) throw new ArgumentNullException("newVent");
                this._vacBarometers.Add(newVent);
            }

            #endregion Mandatory Blocks / VacuumBarometers

            #endregion Mandatory Blocks

            #region Constructors

            private Airlock()
            {
                this._outerDoors = new List<IMyDoor>();
                this._innerDoors = new List<IMyDoor>();
                this._fillVents = new List<IMyAirVent>();
                this._drainVents = new List<IMyAirVent>();
                this._drainTanks = new List<IMyGasTank>();
                this._habBarometers = new List<IMyAirVent>();
                this._vacBarometers = new List<IMyAirVent>();
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

            #region Instance Methods

            /// <summary>
            /// Tests whether this airlock has all the components needed to function.
            /// </summary>
            /// <returns></returns>
            public bool IsComplete()
            {
                bool allGood = true;
                this._badConfigReport = new StringBuilder("Config problems:");

                if (
                    this._outerDoors.Count(x => x.IsFunctional) < 1
                )
                {
                    this._badConfigReport.Append(" No functional Outer Doors.");
                    allGood = false;
                }
                if (
                    this._innerDoors.Count(x => x.IsFunctional) < 1
                )
                {
                    this._badConfigReport.Append(" No functional Inner Doors.");
                    allGood = false;
                }
                if (
                    this._fillVents.Count(x => x.IsFunctional) < 1
                )
                {
                    this._badConfigReport.Append(" No functional Fill Vents.");
                    allGood = false;
                }
                if (
                    this._drainVents.Count(x => x.IsFunctional) < 1
                )
                {
                    this._badConfigReport.Append(" No functional Drain Vents.");
                    allGood = false;
                }
                if (
                    this._drainTanks.Count(x => x.IsFunctional) < 1
                )
                {
                    this._badConfigReport.Append(" No functional Drain Tanks.");
                    allGood = false;
                }
                if (
                    this._habBarometers.Count(x => x.IsFunctional) < 1
                )
                {
                    this._badConfigReport.Append(" No functional Habitat Barometers.");
                    allGood = false;
                }
                if (
                    this._vacBarometers.Count(x => x.IsFunctional) < 1
                )
                {
                    this._badConfigReport.Append(" No functional Vacuum Barometers.");
                    allGood = false;
                }
                return allGood;
            }

            #endregion Instance Methods
        }
    }
}
