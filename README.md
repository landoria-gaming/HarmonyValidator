# Harmony Validator

Harmony Validator checks [Harmony](https://harmony.pardeike.net/) patch targets
during a .NET build. It reads assembly metadata without loading the game or
applying patches. The build fails if a targeted method no longer exists or if
the patch parameters are incorrect.

## Requirements

- A .NET SDK with MSBuild
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
  HarmonyValidator.dll
examples/
  MyTestMod/
    MyTestMod.csproj
    MyTestModPlugin.cs
    CharacterNameLabel.cs
    SetupCharacterPreviewPatch.cs
    UpdateCameraPatch.cs
```

### 1. Install the validator

Extract the release ZIP into your mod repository. Keep
`HarmonyValidator.dll` and `HarmonyValidator.targets` together, for example:

```text
MyMod/
  lib/
    HarmonyValidator/
      HarmonyValidator.dll
      HarmonyValidator.targets
  MyMod.csproj
```

Add the validator path and import to the mod's `.csproj` file:

```xml
<PropertyGroup>
  <HarmonyValidatorPath>$(MSBuildThisFileDirectory)lib\HarmonyValidator</HarmonyValidatorPath>
</PropertyGroup>

<Import Project="$(HarmonyValidatorPath)\HarmonyValidator.targets" />
```

The targets file automatically loads `HarmonyValidator.dll` from the same
directory. It runs the validator after compilation; no command-line invocation
or additional dependency is required.

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
dotnet build MyMod.csproj -p:BepInExPath="path\to\BepInEx" -p:ValheimGamePath="path\to\Valheim"
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

Harmony Validator targets .NET Standard 2.0 and runs inside MSBuild. Mono.Cecil
is merged into `HarmonyValidator.dll`, so only the DLL and targets file are
required on Windows and Linux.

## NuGet releases

After publication on nuget.org, install the stable release with:

```sh
dotnet add MyMod.csproj package HarmonyValidator --version 1.0.0
```

Alternatively, download `HarmonyValidator.1.0.0.nupkg` from the
[v1.0.0 release](https://github.com/landoria-gaming/HarmonyValidator/releases/tag/v1.0.0)
and put it in a local NuGet source directory. Install it with:

```sh
dotnet add MyMod.csproj package HarmonyValidator --version 1.0.0 --source ./local-packages
```

Set `PrivateAssets="all"` on the package reference. The validator targets are
imported automatically; no manual import is needed.

The **Publish release to nuget.org** workflow packages the DLL and targets from
the existing release ZIP, attaches the `.nupkg` to that release, and publishes
it on nuget.org. It runs when a release is published, or manually with a release
tag such as `v1.0.0`.

Publication uses [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
with GitHub OIDC and a temporary API key. No stored API key is required.

On nuget.org, open **Trusted Publishing** from your account menu and create a
GitHub policy with:

| Field | Value |
| --- | --- |
| Repository Owner | `landoria-gaming` |
| Repository | `HarmonyValidator` |
| Workflow File | `release-nuget.yml` (file name only) |
| Environment | Leave empty |
| Package scope | `HarmonyValidator`, with permission to push new packages and versions |

In the GitHub repository, add an Actions **variable** named `NUGET_USER` with
your nuget.org profile username (not your email address). Use the account that
owns the policy or is authorized under its organization. To publish the existing
`1.0.0` release, run **Publish release to nuget.org** manually with `v1.0.0`
after the workflow has been pushed to the default branch.

## Snapshot builds

The [Build snapshot workflow](.github/workflows/snapshot.yml) runs on every push
to `main` and can also be started manually from GitHub's **Actions** tab using
**Run workflow**. It builds Harmony Validator in Release mode with a prerelease
version such as `1.0.0-snapshot.42.1` (run number and attempt).

Download the `HarmonyValidator-snapshot.<run>.<attempt>` artifact from the
completed workflow run. The ZIP contains `HarmonyValidator.dll`, with Mono.Cecil
merged in, `HarmonyValidator.targets`, the license, and a snapshot `.nupkg`.
Extract the DLL and targets into your mod
repository and follow the installation instructions above. Snapshot artifacts
are retained for 30 days.

Alternatively, put the downloaded `.nupkg` in a local NuGet source directory
and install it in your mod project:

```sh
dotnet add MyMod.csproj package HarmonyValidator --version 1.0.0-snapshot.42.1 --source ./local-packages
```

Use the version from the downloaded package and add `PrivateAssets="all"` to
the resulting `PackageReference`. NuGet imports the validator targets
automatically; no manual targets import or `HarmonyValidatorPath` is needed.
The package contains the merged DLL as a build task, with no runtime references
added to the mod. The workflow generates the package as an artifact; it does not
publish it to a NuGet feed.

## License

MIT
