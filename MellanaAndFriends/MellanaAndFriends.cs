using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using System.IO;
using System.Reflection;

namespace MellanaAndFriends;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(LethalLib.Plugin.ModGUID)]
[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class MellanaAndFriends : BaseUnityPlugin
{
    public static MellanaAndFriends Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }
    public static AssetBundle PlushieAssets;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        PlushieAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "plushies"));

        string[] Regulars = ["luna/PlushieLuna", "gerbert/PlushieGerbert", "lana/PlushieLana", "mellana/PlushieMellana", "dude/PlushieDude", "wilbo/PlushieWilbo", "wilbo/PlushieWilmoon"];

        const string BasePath = "Assets/Plushies/";
        const string FileExt = ".asset";
        int iRarity = 15;

        for (int i = 0; i < Regulars.Length; i++) {
            string Path = BasePath + Regulars[i] + FileExt;
            Item? Plushie = PlushieAssets.LoadAsset<Item>(Path);
            if (Plushie == null) {
                Logger.LogError("Failed to find plushie " + Path);
                continue;
            }
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(Plushie.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(Plushie, iRarity, LethalLib.Modules.Levels.LevelTypes.All);
        }

        // int iPrice = 0;
        // TerminalNode iTerminalNode = PlushieAssets.LoadAsset<TerminalNode>("Assets/Plushies/PlushieLuna.asset");
        // LethalLib.Modules.Items.RegisterShopItem(PlushieLuna, null, null, iTerminalNode, iPrice);

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}
