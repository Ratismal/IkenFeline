# IkenFeline

A "ModLoader" that let's you easily patch Ikenfell upon runtime.

I say "ModLoader" in quotation marks because that's all it does. Provides no API or frameworks for mod creation.

## Instructions

Either look at the individual instructions within each project, or

1. Download the latest release
2. Drop the contents into your game's install directory
3. Run `patch.bat` (sorry linux users)
4. Run `MONOMODDED_IkenfellWin.exe`

## TestMod

I included a small test mod in the release which does the following:
- skips splash screens to speed up testing
- adds a "Mods" button to the title screen, but it just opens the regular options menu
- Changes the AbilityIgnite to have infinite range and target everyone, but in the process making you lose control of who you target

## Disclaimer

I have never done anything related to modding or modloading or assembly patching or anything like that in the past and don't know what I'm doing.
