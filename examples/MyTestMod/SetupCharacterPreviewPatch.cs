using HarmonyLib;

namespace MyTestMod;

// Creates the label after Valheim builds a character preview.
[HarmonyPatch(
    typeof(FejdStartup),
    "SetupCharacterPreview",
    new[] { typeof(PlayerProfile) })]
internal static class SetupCharacterPreviewPatch
{
    [HarmonyPostfix]
    private static void Postfix(FejdStartup __instance, PlayerProfile? profile)
    {
        CharacterNameLabel.Create(__instance, profile);
    }
}
