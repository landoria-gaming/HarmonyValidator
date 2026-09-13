# Harmony Validator

Harmony Validator checks [Harmony](https://harmony.pardeike.net/) patch targets
during a .NET build. It reads assembly metadata without loading the game or
applying patches. The build fails if a targeted method no longer exists or if
the patch parameters are incorrect.

## Requirements

- .NET Framework 4.8 for Harmony Validator
- The framework and references required by the mod
- An assembly that uses Harmony

## Valheim Mod Example

This example adds the selected character name above its head in the Valheim
character selection menu.

![Character name above the selected character](examples/MyTestMod/character-name-menu.png)

The complete example is in [`examples/MyTestMod`](examples/MyTestMod/). It uses
.NET Framework 4.8 with BepInEx and Valheim assemblies.

```text
HarmonyValidator/
  HarmonyValidator.targets
  HarmonyValidator.exe
examples/
  MyTestMod/
    MyTestMod.csproj
    MyTestModPlugin.cs
    CharacterNameLabel.cs
    SetupCharacterPreviewPatch.cs
    UpdateCameraPatch.cs
```

### 1. Import the validator

[`MyTestMod.csproj`](examples/MyTestMod/MyTestMod.csproj) defines the BepInEx and
Valheim references, then imports the validator:

```xml
<Import Project="$(HarmonyValidatorPath)\HarmonyValidator.targets" />
```

The example expects these MSBuild properties:

| Property | Value |
| --- | --- |
| `BepInExPath` | BepInEx directory containing `core` |
| `ValheimGamePath` | Valheim installation directory |

### 2. Load the patches

[`MyTestModPlugin.cs`](examples/MyTestMod/MyTestModPlugin.cs) creates the log,
finds the plugin assembly and applies every patch in it:

```csharp
public static ManualLogSource logger =
    BepInEx.Logging.Logger.CreateLogSource(PluginName);

public void Awake()
{
    logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
    Assembly assembly = Assembly.GetExecutingAssembly();
    HarmonyInstance.PatchAll(assembly);
}
```

### 3. Patch a private method

[`CharacterNameLabel.cs`](examples/MyTestMod/CharacterNameLabel.cs) displays the
selected character name above its head. The two Harmony patches each have their
own file and target a private method:

- [`SetupCharacterPreviewPatch.cs`](examples/MyTestMod/SetupCharacterPreviewPatch.cs)
- [`UpdateCameraPatch.cs`](examples/MyTestMod/UpdateCameraPatch.cs)

```csharp
using TMPro;
using UnityEngine;

[HarmonyPatch(
    typeof(FejdStartup),
    "SetupCharacterPreview",
    new[] { typeof(PlayerProfile) })]
internal static class SetupCharacterPreviewPatch
{
    [HarmonyPostfix]
    private static void Postfix(
        FejdStartup __instance,
        PlayerProfile? profile)
    {
        CharacterNameLabel.Create(__instance, profile);
    }
}

[HarmonyPatch(
    typeof(FejdStartup),
    "UpdateCamera",
    new[] { typeof(float) })]
internal static class UpdateCameraPatch
{
    [HarmonyPostfix]
    private static void Postfix(FejdStartup __instance, float dt)
    {
        CharacterNameLabel.Update(__instance);
    }
}
```

### 4. Build the mod

```bat
dotnet build examples\MyTestMod\MyTestMod.csproj -p:BepInExPath="path\to\BepInEx" -p:ValheimGamePath="path\to\Valheim"
```

The validator runs after compilation and before the final DLL is copied:

```text
Harmony targets: 2 checked, 0 errors, 0 unverified.
```

If `UpdateCamera(float)` is incorrectly declared with `string` and `float`, the
build fails:

```text
error SHV002: Target not found: FejdStartup.UpdateCamera
```

## Command line

The validator can also run without MSBuild:

```bat
HarmonyValidator.exe MyTestMod.dll "path\to\BepInEx\core" "path\to\Valheim\valheim_Data\Managed"
```

The last two arguments are assembly search directories. They can point to any
folders containing the mod's dependencies.

A successful validation prints:

```text
Harmony targets: 2 checked, 0 errors, 0 unverified.
```

The process returns exit code `0`. If a patch target or signature is invalid,
the validator reports the error and returns exit code `1`:

```text
HarmonyValidator : error SHV002: Target not found: FejdStartup.UpdateCamera
Harmony targets: 2 checked, 1 errors, 0 unverified.
```

## License

MIT
