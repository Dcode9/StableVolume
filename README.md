# Stable Volume

A native WinUI 3 (Windows App SDK) utility that holds your Windows master volume steady.
Pick a level, flip the switch, and anything that changes it (apps, hotkeys, driver quirks,
device switches) gets snapped back within a blink.

- Lock exactly at a level, or only cap it ("never louder than")
- Optional keep-unmuted
- Follows the default playback device when it changes
- Start with Windows (minimized)
- Uses the Windows Core Audio endpoint volume API (NAudio)

It guards master volume only. It does not normalize loudness between songs or videos.

Download the installer or portable zip from the Releases page ("latest").
