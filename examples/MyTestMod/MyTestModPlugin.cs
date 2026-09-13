using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace MyTestMod;

// Connects the example mod to the BepInEx and Harmony lifecycles.
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class MyTestModPlugin : BaseUnityPlugin
{
    private const string PluginGuid = "com.example.mytestmod";
    private const string PluginName = "My Test Mod";
    private const string PluginVersion = "1.0.0";

    private readonly Harmony HarmonyInstance = new(PluginGuid);

    public static ManualLogSource logger =
        BepInEx.Logging.Logger.CreateLogSource(PluginName);

    // Logs the startup and applies every Harmony patch in this assembly.
    public void Awake()
    {
        logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }

    // Removes patches when BepInEx unloads the plugin.
    public void OnDestroy()
    {
        CharacterNameLabel.Remove();
        HarmonyInstance.UnpatchSelf();
        logger.LogInfo($"{PluginName} {PluginVersion} unloaded.");
    }
}
