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
    public static AssetBundle PlushieAssets;

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        PlushieAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "plushies"));

        const string BasePath = "Assets/Plushies";
        const string FileExt = ".asset";
        
        var categories = new List<(string category, int rarity)>
        {
            ("Regulars", 45),
            ("Test", 0)
        };

        string[] files = PlushieAssets.GetAllAssetNames();
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

                Item? Plushie = PlushieAssets.LoadAsset<Item>(path);
                if (Plushie == null) {
                    Logger.LogError("Failed to load plushie " + path);
                    continue;
                }
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(Plushie.spawnPrefab);
                LethalLib.Modules.Items.RegisterScrap(Plushie, rarity, LethalLib.Modules.Levels.LevelTypes.All);
                Logger.LogMessage("Loaded asset \"" + path + "\"");
            }
            // Remove files that have been evaluated
            files = files.Except(Evaluated).ToArray();
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }
}
