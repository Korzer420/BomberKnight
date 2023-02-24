using BomberKnight.ItemData.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.UnityComponents;

/// <summary>
/// Component to track the amount of jelly eggs / big jellyfish that the player has killed.
/// </summary>
internal class JellyCounter : MonoBehaviour
{
    #region Properties

    public PyromaniacCharmLocation Location { get; set; }

    #endregion

    void Start()
    {
        if (gameObject.name.StartsWith("Jellyfish"))
            On.HealthManager.Die += HealthManager_Die;
        else
            On.JellyEgg.Burst += JellyEgg_Burst;
    }

    void OnDestroy()
    {
        if (gameObject.name.StartsWith("Jellyfish"))
            On.HealthManager.Die -= HealthManager_Die;
        else
            On.JellyEgg.Burst -= JellyEgg_Burst;
    }

    private void JellyEgg_Burst(On.JellyEgg.orig_Burst orig, JellyEgg self)
    {
        if (self.gameObject == gameObject)
            Location.UpdateProgress(gameObject);
        orig(self);
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        if (self.gameObject == gameObject)
            Location.UpdateProgress(gameObject);
        orig(self, attackDirection, attackType, ignoreEvasion);
    }
}
