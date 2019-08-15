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

            /// <summary>
            /// The possible modes of operation (i.e., target states) an Airlock
            /// may be in.
            /// </summary>
            /// <remarks>
            /// <para>
            /// An Airlock in a given mode is not guaranteed to be in exactly
            /// the state described for that mode. But it should be working to
            /// get there.
            /// </para>
            /// </remarks>
            public enum Mode
            {
                /// <summary>
                /// Target pressure matches the Habitat. InnerDoors open,
                /// OuterDoors locked.
                /// </summary>
                OpenToHabitat = 1
                ,
                /// <summary>
                /// Target pressure matches the Vacuum. OuterDoors open,
                /// InnerDoors locked.
                /// </summary>
                OpenToVacuum = 2
                ,
                /// <summary>
                /// Target pressure 0%, all doors locked.
                /// </summary>
                /// <remarks>
                /// <para>
                /// This is not normal operation. It's included in case a
                /// defense-minded engineer wants to be able to trap intruders
                /// between locked doors with nothing to breathe. And, hey, no
                /// one said you can't line your airlock chambers with Interior
                /// Turrets...
                /// </para>
                /// </remarks>
                LockDown = 99
            }

            /// <summary>
            /// The possible target pressure values for the airlock chamber.
            /// </summary>
            public enum PressureTarget
            {
                /// <summary>Zero pressure</summary>
                Empty
                /// <summary>The same pressure reported by the <see cref="VacuumBarometers"/></summary>
                , Vacuum
                /// <summary>The same pressure reported by the <see cref="HabitatBarometers"/></summary>
                , Habitat
                /// <summary>100% pressure</summary>
                , Full
            }

            /// <summary>
            /// The possible target states for <see cref="InnerDoors"/> and <see cref="OuterDoors"/>.
            /// </summary>
            public enum DoorTarget
            {
                /// <summary>Passable and manually operable</summary>
                Open
                /// <summary>Passable and NOT manually operable</summary>
                , LockedOpen
                /// <summary>Impassable, airtight, and manually operable</summary>
                , Closed
                /// <summary>Impassable, airtight, and NOT manually operable</summary>
                , LockedClosed
            }

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

            private Mode _targetMode;
            /// <summary>
            /// The Airlock's Mode of operation. It's either there, or it's working toward it.
            /// </summary>
            public Mode TargetMode {
                get { return this._targetMode; }
                private set { this._targetMode = value; }
            }

            private PressureTarget? _currentPressureTarget;
            /// <summary>
            /// The pressure that the Airlock is currently working toward, or
            /// <code>null</code> if the current pressure is acceptable.
            /// </summary>
            public PressureTarget? CurrentPressureTarget
            {
                get { return this._currentPressureTarget; }
                private set { this._currentPressureTarget = value; }
            }

            private DoorTarget? _currentInnerDoorsTarget;
            /// <summary>
            /// The desired DoorTarget of the <see cref="InnerDoors"/>, or
            /// <code>null</code> if their current states are acceptable.
            /// </summary>
            public DoorTarget? CurrentInnerDoorsTarget
            {
                get { return this._currentInnerDoorsTarget; }
                private set { this._currentInnerDoorsTarget = value; }
            }

            private DoorTarget? _currentOuterDoorsTarget;
            /// <summary>
            /// The desired DoorTarget of the <see cref="OuterDoors"/>, or
            /// <code>null</code> if their current states are acceptable.
            /// </summary>
            public DoorTarget? CurrentOuterDoorsTarget
            {
                get { return this._currentOuterDoorsTarget; }
                private set { this._currentOuterDoorsTarget = value; }
            }

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

            #region Constructors and Factory Methods

            private Airlock()
            {
                this._outerDoors = new List<IMyDoor>();
                this._innerDoors = new List<IMyDoor>();
                this._fillVents = new List<IMyAirVent>();
                this._drainVents = new List<IMyAirVent>();
                this._drainTanks = new List<IMyGasTank>();
                this._habBarometers = new List<IMyAirVent>();
                this._vacBarometers = new List<IMyAirVent>();

                this.TargetMode = Mode.OpenToHabitat;
                this.CurrentPressureTarget = null;
                this.CurrentInnerDoorsTarget = null;
                this.CurrentOuterDoorsTarget = null;
            }

            /// <summary>
            /// Create a new instance of the Airlock class with a given name.
            /// </summary>
            /// <param name="name"></param>
            public Airlock(string name) : this()
            {
                this.Name = name;
            }

            /// <summary>
            /// Search a collection of blocks and create a dictionary of Airlock
            /// objects keyed by Name out of the relevant blocks.
            /// </summary>
            /// <param name="allBlocks">The collection of blocks. The more this
            /// can be narrowed beforehand, the less work this method has to do,
            /// but DiscoverAllAirlocks will effectively sort through a
            /// completely unfiltered set.</param>
            /// <param name="iniParser">Optional reference to a <see
            /// cref="MyIni"/> object that will be used to parse blocks'
            /// CustomData. If absent, the method will create its own.</param>
            /// <param name="iniSectionName">Optional name of the INI section
            /// header that appears in a block's CustomData before airlock
            /// configuration data. If absent, <see
            /// cref="Program.INI_SECTION_NAME"/> will be used.</param>
            /// <returns>A dictionary of all discrete Airlock objects found,
            /// keyed by their Names.</returns>
            /// <remarks>DiscoverAllAirlocks does not check whether the found
            /// Airlocks are complete or functional. However, it will throw
            /// useful exceptions when there is easily-spotted bad configuration
            /// data.</remarks>
            public static IDictionary<string, Airlock> DiscoverAllAirlocks(
                IEnumerable<IMyTerminalBlock> allBlocks
                , MyIni iniParser = null
                , string iniSectionName = null
            )
            {
                if (null == iniParser) iniParser = new MyIni();
                if (null == iniSectionName) iniSectionName = Program.INI_SECTION_NAME;

                // Note the use of CurrentCulture. Airlock names are expected to
                // be user-readable, so the code should obey the user's language
                // rules. But they should still be case-insensitive as that's
                // probably a common assumption.
                Dictionary<string, Airlock> foundAirlocks
                    = new Dictionary<string, Airlock>(StringComparer.CurrentCultureIgnoreCase);

                MyIniParseResult parseResult;
                string airlockName;
                Airlock thisAirlock;
                string roleName;
                foreach (IMyTerminalBlock thisBlock in allBlocks)
                {
                    if (!iniParser.TryParse(thisBlock.CustomData, iniSectionName, out parseResult))
                    {
                        throw new Exception(
                            $"Failed to parse airlock configuration data."
                            + $"\nBlock: {thisBlock.CustomName}"
                            + $"\nParse result: {parseResult.ToString()}"
                        );
                    }

                    // Parse the airlock name and get a new or existing reference to the airlock
                    airlockName = iniParser.Get(iniSectionName, INI_AIRLOCK_KEY).ToString(); // TODO This should not reference Program.INI_AIRLOCK_KEY directly.
                    if (string.IsNullOrEmpty(airlockName))
                    {
                        throw new Exception(
                            $"Failed to parse airlock configuration data because no airlock name was given."
                            + $"\nBlock: {thisBlock.CustomName}"
                        );
                    }

                    if (foundAirlocks.ContainsKey(airlockName))
                    {
                        thisAirlock = foundAirlocks[airlockName];
                    }
                    else
                    {
                        thisAirlock = new Airlock(airlockName);
                        foundAirlocks.Add(airlockName, thisAirlock);
                    }

                    roleName = iniParser.Get(iniSectionName, INI_ROLE_KEY).ToString(); // TODO This should not reference Program.INI_ROLE_KEY directly.
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

                return foundAirlocks;
            }

            #endregion Constructors and Factory Methods

            #region Instance Methods

            /// <summary>
            /// Tests whether this airlock has all the components needed to
            /// function.
            /// </summary>
            /// <returns><code>false</code> if any mandatory components are
            /// missing or non-functional. Otherwise
            /// <code>true</code>.</returns>
            /// <remarks>
            /// <para>
            /// This is a fairly superficial check. In addition to just doing
            /// counts, it will check <see cref="IMyCubeBlock.IsFunctional"/>
            /// and discount any blocks that are too damaged to function.
            /// However, it will not check other important factors, like
            /// sufficient power, or that <see cref="HabitatBarometers"/> face
            /// an airtight space.
            /// </para>
            /// <para>
            /// One might expect <code>IsComplete</code> to ensure that each
            /// block is Enabled, but it does not. This is because the script
            /// will automatically disable and enable certain blocks during
            /// normal operation.
            /// </para>
            /// </remarks>
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
