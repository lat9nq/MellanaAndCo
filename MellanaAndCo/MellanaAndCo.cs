using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace MellanaAndCo;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID)]
public class MellanaAndCo : BaseUnityPlugin
{
    public static MellanaAndCo Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    readonly List<(string, int)> categories =
    [
        ("Regulars", 45),
        ("Extra", 40),
        ("Test", 0)
    ];

    private void LoadBundle(string sBundlePath) {
        AssetBundle? Bundle = AssetBundle.LoadFromFile(sBundlePath);

        if (Bundle == null) {
            Logger.LogInfo("Failed to load bundle \"" + Path.GetFileName(sBundlePath) + "\", skipping" );
            return;
        }

        const string BasePath = "Assets/MellanaAndCo";
        const string FileExt = ".asset";

        string[] files = Bundle.GetAllAssetNames();
        foreach (var (category, rarity) in categories) {
            string[] Evaluated = [];
            foreach (string path in files) {
                if (!path.Contains(FileExt)) {
                    Evaluated.AddItem(path);
                    Logger.LogDebug("Culled " + path);
                    continue;
                }
                if (!path.Contains(BasePath + "/" + category, System.StringComparison.CurrentCultureIgnoreCase)) {
                    Logger.LogDebug("Skipped " + path);
                    continue;
                }
                Evaluated.AddItem(path);

                Item? Plushie = Bundle.LoadAsset<Item>(path);
                if (Plushie == null) {
                    Logger.LogError("Failed to load plushie " + path);
                    continue;
                }
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(Plushie.spawnPrefab);
                LethalLib.Modules.Items.RegisterScrap(Plushie, rarity, LethalLib.Modules.Levels.LevelTypes.All);
                Logger.LogInfo("Loaded asset \"" + path + "\"");
            }
            // Remove files that have been evaluated
            files = files.Except(Evaluated).ToArray();
        }
    }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        // Load included asset bundle
        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string sBundlePath = Path.Combine(sAssemblyLocation, "mellanaandco");
        LoadBundle(sBundlePath);

        // Load optional extras bundle
        string sExtraBundlePath = Path.GetFullPath("../toastUnlimited-MellanaAndCoExtras/mellanaandco_extras", sAssemblyLocation);
        LoadBundle(sExtraBundlePath);

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }
}
