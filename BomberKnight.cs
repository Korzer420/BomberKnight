using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using BomberKnight.UnityComponents;
using Modding;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BomberKnight.ItemData.Locations;
using BomberKnight.SaveManagement;
using System.Linq;
using BomberKnight.ItemData;
using BomberKnight.ModInterop;
using ItemChanger;
using BomberKnight.BombElements;

namespace BomberKnight;

public class BomberKnight : Mod, IGlobalSettings<GlobalSaveData>, ILocalSettings<LocalSaveData>, IMenuMod
{
    #region Constants

    public const string PyromaniacCharm = "Pyromaniac";
    public const string ShellSalvagerCharm = "ShellSalvager";
    public const string BombMasterCharm = "BombMaster";

    #endregion

    #region Constructor

    public BomberKnight()
    => SFCore.InventoryHelper.AddInventoryPage(SFCore.InventoryPageType.Empty, "Bomber Knight", "Bomber_Knight", "Bomber_Knight", "HasBombBag", BombUI.CreateInventoryPage);

    #endregion

    #region Properties

    public static BomberKnight Instance { get; set; }

    public bool ToggleButtonInsideMenu => false;

    #endregion

    #region Methods

    public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public override List<(string, string)> GetPreloadNames()
         => new()
         {
             ("Ruins1_05c", "Ceiling Dropper (4)"), // The explosion
             ("Ruins1_05c", "Ruins Sentry Fat"), // For the bridge fight
             ("Ruins1_24_boss", "Mage Lord"), // For the edge fight (shockwave)
             ("Deepnest_39", "Spider Flyer (1)") // For the deepnest fight
         };

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Instance = this;
        BombCharms.Initialize();
        BombManager.Initialize();
        BombDrop.Initialize();
        ItemManager.Initialize();
        Bomb.Explosion = preloadedObjects["Ruins1_05c"]["Ceiling Dropper (4)"].LocateMyFSM("Ceiling Dropper")
            .GetState("Explode")
            .GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
        GameObject.DontDestroyOnLoad(Bomb.Explosion);
        EdgeBombBagLocation.Shockwave = preloadedObjects["Ruins1_24_boss"]["Mage Lord"].LocateMyFSM("Mage Lord")
            .GetState("Quake Waves")
            .GetFirstActionOfType<SpawnObjectFromGlobalPool>()
            .gameObject.Value;
        GameObject.DontDestroyOnLoad(EdgeBombBagLocation.Shockwave);
        BounceBombLocation.Sentry = preloadedObjects["Ruins1_05c"]["Ruins Sentry Fat"];
        DeepnestBombBagLocation.Spider = preloadedObjects["Deepnest_39"]["Spider Flyer (1)"];
        if (ModHooks.GetMod("DebugMod") is Mod)
            HookDebug();
    }

    private void HookDebug()
    {
        DebugInterop.Initialize();
    }

    public void OnLoadGlobal(GlobalSaveData saveData)
    {
        if (saveData == null)
            return;
        BombManager.ColorlessHelp = saveData.ColorlessHelp;
        BombSpell.UseCast = saveData.BombFromCast;
    }

    public GlobalSaveData OnSaveGlobal() => new()
    {
        BombFromCast = BombSpell.UseCast,
        ColorlessHelp = BombManager.ColorlessHelp,
    };

    public void OnLoadLocal(LocalSaveData saveData)
    {
        if (saveData.AvailableBombTypes != null)
            BombManager.AvailableBombs = saveData.AvailableBombTypes;
        if (saveData.CharmData != null)
        {
            foreach (CharmData key in saveData.CharmData)
            {
                CharmData charmData = BombCharms.CustomCharms.FirstOrDefault(x => x.Name == key.Name);
                if (charmData == null)
                    BombCharms.CustomCharms.Add(charmData);
                else
                {
                    charmData.Equipped = key.Equipped;
                    charmData.Acquired = key.Acquired;
                    charmData.Cost = key.Cost;
                }
            }
        }
        if (saveData.Inventory != null)
            BombManager.SetBombsSilent(saveData.Inventory);
        BombManager.BombBagLevel = saveData.BombBagLevel;
        if (saveData.KnightOrder != null)
            ShellSalvagerLocation.ChestOrder = saveData.KnightOrder;
    }

    public LocalSaveData OnSaveLocal() => new()
    {
        AvailableBombTypes = BombManager.AvailableBombs,
        BombBagLevel = BombManager.BombBagLevel,
        Inventory = BombManager.BombQueue.ToList(),
        KnightOrder = ShellSalvagerLocation.ChestOrder,
        CharmData = BombCharms.CustomCharms
    };

    public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
    {
        return new()
        {
            new("Drop Button", new string[]{"Cast", "Quickcast"}, "Determines which button should be used to drop bombs.", x => BombSpell.UseCast = x == 0, () => BombSpell.UseCast ? 0 : 1),
            new("Colorless Indicator", new string[]{"Off", "On"}, "If on, additional info is displayed for accessability", x => 
            {
                BombManager.ColorlessHelp = x == 1;
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Main_Menu")
                    BombUI.UpdateTracker();
            }, () => BombManager.ColorlessHelp ? 1 : 0)
        };
    }

    #endregion
}