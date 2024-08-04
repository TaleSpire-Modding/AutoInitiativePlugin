# Auto Initiative
This plugin allows pulling the result of url invoked TS Dice rolls (Via browser or Symbiote) and update initiative tracking.
All Rolls need "Initiative" in the name to be picked up by the plugin (Case insensitive, Configurable).
This mod was inspired by LA's original AutoInitiative plugin, the changes here are as follows:
- Only the GM needs it installed
- No Keybinds needed (Automatically pulls Die Results into initiative tracker when GM is editing Turn Order)
- Reduced Netcode footprint and simpler install
- GM's can roll initiative for an individual/group using a shared initiative modifier
- Tie breaker for Initiative order is automated by the flat initiative modifier

## Install
This uses Bepinex and requires SetInjectionFlag plugin installed.

## Usage
1. Select mini(s) that you will be rolling initiative for
2. GM Open switches to Turnbase Mode and enable the Initiative Tracker editor
3. Tell players to roll initiative with their mini selected. Roll must include "Initiative" in the name.
4. Roll Initiative for first mini(s)
5. Repeat until all minis have their initiative rolled (As you roll, they will be tracked in the Initiative Tracker)

GM's can also lasso multiple minis and roll initiative for all of them at once using the shared result.
Tied Initiatives are broken by their initiaitve bonus (If they have one).
All Rolls need "Initiative" in the name to be picked up by the plugin (Case insensitive).

## Changelog
- 1.0.0: Initial release

## Who Needs The Plugin
Only the GM needs to have the plugin installed. The Gameclient uses the di(c)e roll results to track 