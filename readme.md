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
- Changes the AbilityFireball to have infinite range and target everyone. And I mean, _everyone_.

## Disclaimer

I have never done anything related to modding or modloading or assembly patching or anything like that in the past and don't know what I'm doing.

## Developers

There's multiple ways you can utilize IkenFeline.
1. Create regular monomod patches and put them in the mod folder
1. Reference the included `MMHOOK_LittleWitch.dll` and `MMHOOK_GameEngine.dll` for easier hooking of functions [see HookGen docs](https://github.com/MonoMod/MonoMod/blob/master/README-RuntimeDetour.md#using-hookgen)
1. Create an `IkenFelineMod` class

You can do any or all of the above. An `IkenFelineMod` class is probably preferable going forward, as it'll provide infrastructure with IkenFeline - just remember to reference `IkenFeline.dll` in your project!

Best practices would be to include all of your files within a single directory inside the mod folder. So for example,
```
- IkenFeline
  - mods
    - YourMod
      - YourMod.dll
      - LittleWitch.YourMod.mm.dll
      - GameEngine.YourMod.mm.dll
```

### Patches

Patches are the classic IkenFeline way, by just directly throwing your patches into the respective assemblies. For example, if you wanted to patch Maritte's ignite ability to, say, have infinite range, you could do this:
```cs
namespace LittleWitch
{
  public class patch_AbilityIgnite : AbilityIgnite
  {
    // this is the original method that you're overwriting. you may call as needed
    public extern bool orig_InRange(BattleGrid grid, BattleUnit unit, VectorI target);
    public override bool InRange(BattleGrid grid, BattleUnit unit, VectorI target)
    {
      return true;
    }

    public override bool InHitArea(BattleGrid grid, BattleUnit unit, VectorI target, VectorI tile)
    {
      return true;
    }
  }
}
```

### HookGen

HookGen is a monomod method of creating runtime detours. This allows you to make modifications without making patches, which IMO is much simpler. I'm not sure if it can private stuff? And redefining variables might like, not be a thing. But it's definitely much much quicker and easier to use. And would go nicely with the IkenFelineMod classes.

For example, if you wanted to hook Maritte's fireball ability to, say, have infinite range, you could do this:

```cs
On.LittleWitch.AbilityFireball.InHitArea += (orig, ability, grid, unit, target, tile) =>
{
    return true;
};
On.LittleWitch.AbilityFireball.InRange += (orig, ability, grid, unit, target) =>
{
    return true;
};
```

Make sure you're referencing `MMHOOK_LittleWitch.dll` or `MMHOOK_GameEngine.dll` in your project!

### IkenFelineMod

An IkenFelineMod class is a good base going forwards, to standardize how mods will get loaded into IkenFeline. Here's an example:

```cs
// Takes three parameters, a modid, mod name, and version
[IkenFelineMod("com.username.your-cool-mod", "Your Cool Mod", "1.0")]
public class YourCoolMod : IkenFelineMod {
  public override void Load() {
    Logger.Log("Hello, world!");
  }
}
```

This class can be in any of your DLLs. So for example if you're patching LittleWitch, you could include this class there. Or, you could make a brand new DLL to put it in. The world's your oyster.

If you put your mod class in a standalone assembly that's not referenced by any `*.*.mm.dll` files or any other assembly that IkenFeline loads, you _must_ name your assembly `ModName.dll` and put it in a folder called `ModName` (obviously replacing ModName with your mod's name), or it won't get loaded! Like so:
```
IkenFeline
  - Mods
    - ModName
      - ModName.dll
```

The reason for this is that it minimizes the number of assemblies IkenFeline has to look through to find mod classes, and it paves the way for in the future where other kind of loading (ex. content, scripts) will take place.