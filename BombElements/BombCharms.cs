using BomberKnight.Enums;
using BomberKnight.ItemData;
using BomberKnight.Resources;
using KorzUtils.Helper;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using SF = SFCore;

namespace BomberKnight.BombElements;

/// <summary>
/// Handles everything around the new charms.
/// </summary>
internal static class BombCharms
{
    #region Constants

    private const string GotCharmPrefix = "gotCharm_";
    private const string EquippedCharmPrefix = "equippedCharm_";
    private const string CharmCostPrefix = "charmCost_";
    private const string CharmNamePrefix = "CHARM_NAME_";
    private const string CharmDescPrefix = "CHARM_DESC_";

    #endregion

    #region Properties

    public static List<CharmData> CustomCharms { get; } = new();

    #endregion

    #region Event handler

    private static int ModHooks_SetPlayerIntHook(string name, int orig)
    {
        if (name.StartsWith(CharmCostPrefix))
            if (CheckCustomCharm(name, CharmCostPrefix) is CharmData charmData)
                charmData.Cost = orig;
        return orig;
    }

    private static bool ModHooks_SetPlayerBoolHook(string name, bool orig)
    {
        if (name.StartsWith(GotCharmPrefix))
        {
            if (CheckCustomCharm(name, GotCharmPrefix) is CharmData charmData)
                charmData.Acquired = orig;
        }
        else if (name.StartsWith(EquippedCharmPrefix))
        {
            if (CheckCustomCharm(name, EquippedCharmPrefix) is CharmData charmData)
                charmData.Equipped = orig;
        }
        return orig;
    }

    private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key.StartsWith(CharmNamePrefix))
        {
            if (CheckCustomCharm(key, CharmNamePrefix) is CharmData charmData)
                orig = InventoryText.ResourceManager.GetString($"Charm_{charmData.Name}_Title");
        }
        else if (key.StartsWith(CharmDescPrefix))
        {
            if (CheckCustomCharm(key, CharmDescPrefix) is CharmData charmData)
                orig = InventoryText.ResourceManager.GetString($"Charm_{charmData.Name}_Desc");
        }
        return orig;
    }

    private static int ModHooks_GetPlayerIntHook(string name, int orig)
    {
        if (name.StartsWith(CharmCostPrefix))
        {
            if (CheckCustomCharm(name, CharmCostPrefix) is CharmData charmData)
                orig = charmData.Cost;
        }
        return orig;
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name.StartsWith(GotCharmPrefix))
        {
            if (CheckCustomCharm(name, GotCharmPrefix) is CharmData charmData)
                orig = charmData.Acquired;
        }
        else if (name.StartsWith(EquippedCharmPrefix))
        {
            if (CheckCustomCharm(name, EquippedCharmPrefix) is CharmData charmData)
                orig = charmData.Equipped;
        }
        return orig;
    }

    #endregion

    #region Control

    internal static void Initialize()
    {
        CharmHelper.AddCustomCharm(BomberKnight.PyromaniacCharm, SF.CharmHelper.AddSprites(SpriteHelper.CreateSprite<BomberKnight>("Sprites." + BomberKnight.PyromaniacCharm))[0]);
        CharmHelper.AddCustomCharm(BomberKnight.ShellSalvagerCharm, SF.CharmHelper.AddSprites(SpriteHelper.CreateSprite<BomberKnight>("Sprites." + BomberKnight.ShellSalvagerCharm))[0]);
        CharmHelper.AddCustomCharm(BomberKnight.BombMasterCharm, SF.CharmHelper.AddSprites(SpriteHelper.CreateSprite<BomberKnight>("Sprites." + BomberKnight.BombMasterCharm))[0]);

        CustomCharms.Add(new()
        {
            Id = CharmHelper.GetCustomCharmId(BomberKnight.PyromaniacCharm),
            Name = BomberKnight.PyromaniacCharm,
            Cost = 4
        });
        CustomCharms.Add(new()
        {
            Id = CharmHelper.GetCustomCharmId(BomberKnight.ShellSalvagerCharm),
            Name = BomberKnight.ShellSalvagerCharm,
            Cost = 1
        });
        CustomCharms.Add(new()
        {
            Id = CharmHelper.GetCustomCharmId(BomberKnight.BombMasterCharm),
            Name = BomberKnight.BombMasterCharm,
            Cost = 2
        });

        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;
        ModHooks.SetPlayerIntHook += ModHooks_SetPlayerIntHook;
    }

    #endregion

    #region Private Methods

    private static CharmData CheckCustomCharm(string key, string prefix)
    {
        try
        {
            if (!int.TryParse(key.Substring(prefix.Length), out int charmId))
                // Unbreakable charms end with _G
                charmId = Convert.ToInt32(key.Substring(prefix.Length, 2));
            return CustomCharms.FirstOrDefault(x => x.Id == charmId);
        }
        catch (Exception)
        {
            LogHelper.Write<BomberKnight>("Tried requesting stuff for unknown key: " + key + " with prefix " + prefix, KorzUtils.Enums.LogType.Warning);
            return null;
        }
    }

    #endregion
}
