using HarmonyLib;

namespace MyTestMod;

// Updates the label while Valheim moves the private menu camera.
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
