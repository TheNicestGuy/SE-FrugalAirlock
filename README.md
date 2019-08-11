# SE-FrugalAirlock (a.k.a. TNG's Frugal Airlock)

## An airlock-management script for the game Space Engineers ##

TNG's Frugal Airlock is a C# script meant for a Programmable Block in the game
<i>Space Engineers</i> by Keen Software House. It enables the setup of airlocks:
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
   drainage tanks), but beyond those it doesn't care about size, shape, or
   style. As for the method of engineer interaction, the script makes it extra
   easy to set up common arrangements like button panels or sensors. For the
   daring, there is also an API of commands you can use to set up fancier
   interfaces and integrations.

3. _The player should not have to touch the script._ Configuring the script to
   work with your design is done by adding INI-style text to the CustomData of
   your various airlock-related blocks, from a well-commented template. There is
   no need to edit the script with a list of block or group names, nor is any
   naming convention imposed.

Complete details are in [the wiki](https://github.com/TheNicestGuy/SE-FrugalAirlock/wiki).

## How to Use this Repo

Most end users will just want to go straight to the
[Releases](https://github.com/TheNicestGuy/SE-FrugalAirlock/releases) and
download the latest ZIP, but _not_ the one marked as "Source Code". The release
ZIP contains a "pre-cooked", ready-to-eat `script.cs` file, and a `Template.ini`
file that provides a guide to configuring your airlocks. To use, just paste
`script.cs` into a Programmable Block. `Template.ini` will explain its own use,
basically, but you'll want to read the [the
wiki](https://github.com/TheNicestGuy/SE-FrugalAirlock/wiki) for a complete
understanding.

TO DO: This should eventually be available on the Steam Workshop for even easier
use. It's not mature enough for that yet.

If you're more interested in the source code and guts, you'll need not only
Microsoft Visual Studio (this solution was created in 2019 Community Edition),
but also the [MDK-SE](https://github.com/malware-dev/MDK-SE) extension for
Visual Studio by malware-dev. MDK-SE does the "cooking" of an organized codebase
into a single <i>Space Engineers</i> script, along with lots of other
conveniences. It's absolutely indispensible for anyone serious about <i>Space
Engineers</i> scripting.