# Auto Initiative
This plugin allows you to pull the result of URL-invoked TS Dice rolls (via browser or Symbiote) and update initiative tracking.
All Rolls need "Initiative" in the name to be picked up by the plugin (case-insensitive, Configurable).
This mod was inspired by LA's original AutoInitiative plugin; the changes here are as follows:
- Only the GM needs it installed
- No Keybinds needed (Automatically pulls Die Results into initiative tracker when GM is editing Turn Order)
- Reduced Netcode footprint and simpler install
- GMs can roll initiative for an individual/group using a shared initiative modifier
- The flat initiative modifier automates the tie breaker for Initiative order

## Install
This uses Bepinex and requires the SetInjectionFlag plugin installed.

## Usage
1. Select mini(s) that you will be rolling initiative for
2. GM Open switches to Turnbase Mode and enables the Initiative Tracker editor
3. Tell players to roll initiative with their mini selected. Roll must include "Initiative" in the name.
4. Roll Initiative for first mini(s)
5. Repeat until all minis have their initiative rolled (As you roll, they will be tracked in the Initiative Tracker)

GMs can also lasso multiple minis and roll initiative for all of them at once using the shared result.
Tied Initiatives are broken by their initiative bonus (If they have one).
All Rolls need "Initiative" in the name to be picked up by the plugin (case-insensitive).

## Changelog
- 1.2.1: DependencyUnityPlugins is now used and implement logic for unpatching
- 1.2.0: Fix broken changes from TaleSpire dice update
- 1.1.0: AOE Initiative Update
- 1.0.0: Initial release

## Who Needs The Plugin
Only the GM needs to have the plugin installed. The Gameclient uses the di(c)e roll results to track 

## Shoutouts
<!-- CONTRIBUTORS-START -->
Shoutout to my past [Patreons](https://www.patreon.com/HolloFox) and [Discord](https://discord.gg/up6sWSjr) members, recognising your mighty support and contribution to my caffeine addiction:
- [Demongund](https://www.twitch.tv/demongund) - Introduced me to TaleSpire
- [Tales Tavern/MadWizard](https://talestavern.com/)
- Joaqim Planstedt
<!-- CONTRIBUTORS-END -->
