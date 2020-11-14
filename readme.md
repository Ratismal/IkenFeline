# IkenFeline

A "ModLoader" that let's you easily patch Ikenfell upon runtime.

I say "ModLoader" in quotation marks because that's all it does. Provides no API or frameworks for mod creation.

## Instructions

Either look at the individual instructions within each project, or

1. Download the latest release
2. Drop the contents into your game's install directory
3. Run `patch.bat` (sorry linux users)
4. Run `MONOMODDED_IkenfellWin.exe`

To get it to work with Steam, overwrite `IkenfellWin.exe` with your `MONOMODDED_IkenfellWin.exe` file. I'd recommend making a backup first.

## TestMod

I included a small test mod in the release which does the following:
- skips splash screens to speed up testing
- adds a "Mods" button to the title screen, but it just opens the regular options menu
- Changes the AbilityIgnite to have infinite range and target everyone, but in the process making you lose control of who you target
- Changes the AbilityFireball to have infinite range and target everyone. And I mean, _everyone_.

## Disclaimer

I have never done anything related to modding or modloading or assembly patching or anything like that in the past and don't know what I'm doing.

## Developers

When making your mod, you need to make sure that you use a proper layout. You need a folder, assemblies/other content in the folder, and a manifest.yaml file. An example setup might look like this:
```
- IkenFeline
  - mods
    - YourMod
      - YourMod.dll
      - LittleWitch.YourMod.mm.dll
      - GameEngine.YourMod.mm.dll
      - manifest.yaml
```

### Manifest

The first thing you need to do to get started with IkenFeline is create a manifest file. This will tell IkenFeline information about your mod, as well as how it should be loaded. For example,
```yaml
ModId: me.stupidcat.testmod # the ID of your mod, can be anything as long as it's UNIQUE
Name: Test Mod
Version: 0.0.1
Description: A test mod to demonstrate the capabilities of IkenFeline
Main: LittleWitch.TestMod.mm.dll # this must be the location of the assembly that your mod class resides in
MainClass: TestMod.TestMod # this must be the full location to your mod class (including namespaces)
Patches: # an optional array of patches. note that patches can reside in your main assembly!
- LittleWitch.TestMod.mm.dll
```

It's very important that you include the Main and MainClass properties, and that they reference a class extending from IkenFelineMod, or your mod won't get loaded!

### Mod Class

You must expose a Mod Class that extends from IkenFelineMod. This will serve as your base of operations, and is where IkenFeline will send you events and hooks and stuff. Here's an example:
```cs
public class YourCoolMod : IkenFelineMod
{
  public static YourCoolMod Instance;

  public YourCoolMod(ModManifest manifest) : base(manifest)
  {
    Instance = this;
  }

  public override void Load()
  {
      Logger.Log("Hello, world!");
  }
}
```

You can then access this class any time by using `YourCoolMod.Instance`!

There are currently a few functions that you can hook into for lifecycle events
- `Load` - called immediately after your mod loads
- `PostLoad` - called after all mods have loaded
- `PreGameLoad` - called before the game loads
- `PostGameLoad` - called after the game finishes loading

More to come!

### Making Changes

Now that you've got the initial setup done, there are two ways that you can utilize IkenFeline
1. Using event hooks (recommended)
1. Patching files directly (advanced)

#### Event Hooks (recommended)

Runtime Detours (hooks) are a monomod method of safely creating overrides to base functionality. The system was designed with multiple mods in mind, so this is the recommended method of creating modifications. This allows you to make modifications without making patches, which a lot simpler, quicker, and easier to use. It does come with some limitations, so in certain cases you may need to go with patches instead.

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

#### Patches (advanced)

Patches are the classic IkenFeline/MonoMod way, by just directly throwing your patches into the respective assemblies. This can be potentially dangerous, however, as your patches may conflict with other mods. It also prevents you from using any sort of debugging, unless someone can figure out how to generated pdb files from monomod patching. I'd recommend using the hooks below, unless this is absolutely necessary.

For example, if you wanted to patch Maritte's ignite ability to, say, have infinite range, you could do this:
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
