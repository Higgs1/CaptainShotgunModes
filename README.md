# Caution! Disclaim!!1

This is my custom fork of [Vl4dimyr](https://github.com/Vl4dimyr)'s **[CaptainShotgunModes](https://github.com/Vl4dimyr/CaptainShotgunModes)** for me and my furiend group. You'll probably want to get the [OG version](https://thunderstore.io/package/Vl4dimyr/CaptainShotgunModes) for yourself. You won't get any support for this version. You've been warned!

## Compiling

- Something
- Something
- Something
- `msbuild -nologo -m -r`

# CaptainShotgunModes

## Description

This mod allows you to choose between 3 firing modes for the captain's shotgun: `Normal`, `Auto` and `AutoCharge`.
With both auto modes you can just hold down the fire key and don´t have to spam it.

The modes can be selected using the number keys `1`, `2` and `3`, or cycled through using the `mouse wheel` or [DPad](https://en.wikipedia.org/wiki/D-pad).
The current mode is displayed above the primary skill icon.

## Modes

| Mode       | Key | Description | Screenshot |
|------------|-----|-------------|------------|
| Normal     |  1  | The default mode/behavior of the captain's shotgun. | ![normal](https://raw.githubusercontent.com/Vl4dimyr/CaptainShotgunModes/master/images/sc_normal.jpg)
| Auto       |  2  | Automatically fires as long as the fire key is pressed. | ![auto](https://raw.githubusercontent.com/Vl4dimyr/CaptainShotgunModes/master/images/sc_auto.jpg)
| AutoCharge |  3  | Automatically fires after charging as long as the fire key is pressed. | ![auto_charge](https://raw.githubusercontent.com/Vl4dimyr/CaptainShotgunModes/master/images/sc_auto_charge.jpg)

### Cycle through modes

| Direction | Actions                                 |
|-----------|-----------------------------------------|
| Forward   | Mouse Wheel Down, DPad Right, DPad Down |
| Backward  | Mouse Wheel Up, DPad Left, DPad Up      |

> I did add controller support because enough players wanted it! [see](https://github.com/Vl4dimyr/CaptainShotgunModes/issues/1).

> Currently this is only tested with a xbox 360 controller (xbox one should work too).

## Config

The config file (`\BepInEx\config\de.userstorm.captainshotgunmodes.cfg`) will be crated automatically when the mod is loaded.
You need to restart the game for changes to apply in game.

### Example config

The example config keeps only mode selection with number keys enabled and sets the default mode to `Auto`.

```ini
## Settings file was created by plugin CaptainShotgunModes v1.1.0
## Plugin GUID: de.userstorm.captainshotgunmodes

[Settings]

## The mode that is selected on game start. Modes: Normal, Auto, AutoCharge
# Setting type: String
# Default value: Normal
DefaultMode = Auto

## When set to true modes can be selected using the number keys
# Setting type: Boolean
# Default value: true
EnableModeSelectionWithNumberKeys = true

## When set to true modes can be cycled through using the mouse wheel
# Setting type: Boolean
# Default value: true
EnableModeSelectionWithMouseWheel = false

## When set to true modes can be cycled through using the DPad (controller)
# Setting type: Boolean
# Default value: true
EnableModeSelectionWithDPad = false
```

## Manual Install

- Install [BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/) and [R2API](https://thunderstore.io/package/tristanmcpherson/R2API/)
- Download the latest `CaptainShotgunModes_x.y.z.zip` TODO
- Extract and move the `CaptainShotgunModes.dll` into the `\BepInEx\plugins` folder
