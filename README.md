# SE-FrugalAirlock (a.k.a. TNG's Frugal Airlock)

## An airlock-management script for the game Space Engineers ##

TNG's Frugal Airlock is a C# script meant for a Programmable Block in the game
_Space Engineers_ by Keen Software House. It enables the setup of airlocks:
enclosed, airtight spaces that allow engineers to easily pass between low-oxygen
zones (i.e., "vacuum") and breathable zones (i.e., "habitat").

Compared to other airlock strategies, TNG's Frugal Airlock places priority on a
few key principles, in this order:

1. _No oxygen is ever wasted._ When properly set up, an airlock will never vent
   any O<sub>2</sub> into the vacuum, nor will it use up more O<sub>2</sub> from
   a main supply than necessary.

2. _The form factor, ergonomics, and user interface of your airlock are up to
   you._ The script requires a few obligatory elements to be in place (obvious
   things like Air Vents and an airtight structure and less obvious things like
   drainage tanks), but beyond those it doesn't care about size, shape, or the
   method of engineer interaction.

3. _The player should not have to touch the script._ Configuring the script to
   work with your design is done by adding INI-style text to the CustomData of
   your various airlock-related blocks, from a well-commented template. There is
   no need to edit the script with a list of block or group names, nor is any
   naming convention imposed.

Complete details are in [the wiki](wiki).