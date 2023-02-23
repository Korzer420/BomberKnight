using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using BomberKnight.UnityComponents;
using Modding;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BomberKnight.ItemData.Locations;

namespace BomberKnight;

public class BomberKnight : Mod
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
    }

    #endregion
}