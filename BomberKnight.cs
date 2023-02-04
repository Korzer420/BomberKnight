using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using LoreMaster.UnityComponents;
using Modding;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BomberKnight;

public class BomberKnight : Mod
{
    #region Properties

    public static BomberKnight Instance { get; set; }

    #endregion

    #region Methods

    public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public override List<(string, string)> GetPreloadNames()
         => new()
         {
             ("Ruins1_01", "Ceiling Dropper")
         };

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Instance = this;
        BombManager.Initialize();
        Bomb.Explosion = preloadedObjects["Ruins1_01"]["Ceiling Dropper"].LocateMyFSM("Ceiling Dropper")
            .GetState("Explode")
            .GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
        GameObject.DontDestroyOnLoad(Bomb.Explosion);
    }

    #endregion
}