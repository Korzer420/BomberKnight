using BomberKnight.ItemData.Locations;
using ItemChanger;
using ItemChanger.Items;
using KorzUtils.Helper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BomberKnight.ItemData;

/// <summary>
/// Manages all item related stuff.
/// </summary>
public static class ItemManager
{
    #region Constants

    #region Location names

    public const string ShellSalvagerCharmPuzzle = "Shell_Salvager_Charm-Puzzle";
    public const string PyromaniacCharmPuzzle = "Pyromaniac_Charm-Puzzle";
    public const string BombMasterCharmPuzzle = "Bomb_Master_Charm-Puzzle";
    public const string GreenpathBombBagChallenge = "Greenpath_Bomb_Bag-Challenge";
    public const string DeepnestBombBagChallenge = "Deepnest_Bomb_Bag-Challenge";
    public const string EdgeBombBagChallenge = "Edge_Bomb_Bag-Challenge";
    public const string EchoBombPuzzle = "Echo_Bomb-Puzzle";
    public const string GoldBombPuzzle = "Gold_Bomb-Puzzle";
    public const string SporeBombPuzzle = "Spore_Bomb-Puzzle";
    public const string BounceBombPuzzle = "Bounce_Bomb-Puzzle";
    public const string PowerBombPuzzle = "Power_Bomb-Puzzle";

    #endregion

    #region Item names

    public const string ShellSalvagerCharm = "Shell_Salvager_Charm";
    public const string PyromaniacCharm = "Pyromaniac_Charm";
    public const string BombMasterCharm = "Bomb_Master_Charm";
    public const string BombBag = "Bomb_Bag";
    public const string BounceBomb = "Bounce_Bomb";
    public const string SporeBomb = "Spore_Bomb";
    public const string GoldBomb = "Gold_Bomb";
    public const string EchoBomb = "Echo_Bomb";
    public const string PowerBomb = "Power_Bomb";

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Determines the order of the shell salvage puzzle.
    /// </summary>
    internal static int Seed { get; set; } = -1;

    #endregion

    #region Methods

    internal static void Initialize()
    {
        using Stream locationStream = ResourceHelper.LoadResource<BomberKnight>("ItemChangerData.Locations.json");
        using StreamReader reader = new(locationStream);
        JsonSerializer jsonSerializer = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        foreach (AbstractLocation location in jsonSerializer.Deserialize<List<AbstractLocation>>(new JsonTextReader(reader)))
            Finder.DefineCustomLocation(location);

        using Stream itemStream = ResourceHelper.LoadResource<BomberKnight>("ItemChangerData.Items.json");
        using StreamReader reader2 = new(itemStream);

        foreach (AbstractItem item in jsonSerializer.Deserialize<List<AbstractItem>>(new JsonTextReader(reader2)))
        {
            if (item.name == ShellSalvagerCharm)
                (item as BoolItem).fieldName += CharmHelper.GetCustomCharmId(BomberKnight.PyromaniacCharm);
            else if (item.name == PyromaniacCharm)
                (item as BoolItem).fieldName += CharmHelper.GetCustomCharmId(BomberKnight.PyromaniacCharm);
            else if (item.name == BombMasterCharm)
                (item as BoolItem).fieldName += CharmHelper.GetCustomCharmId(BomberKnight.BombMasterCharm);
            Finder.DefineCustomItem(item);
        }
    }

    internal static void GeneratePlacements()
    {
        ItemChangerMod.CreateSettingsProfile(false);
        List<AbstractPlacement> placements = new()
        {
            // Bomb bags
            CreatePlacement(BombBag, GreenpathBombBagChallenge),
            CreatePlacement(BombBag, DeepnestBombBagChallenge),
            CreatePlacement(BombBag, EdgeBombBagChallenge),
            // Bombs
            CreatePlacement(BounceBomb, BounceBombPuzzle),
            CreatePlacement(SporeBomb, SporeBombPuzzle),
            CreatePlacement(GoldBomb, GoldBombPuzzle),
            CreatePlacement(EchoBomb, EchoBombPuzzle),
            CreatePlacement(PowerBomb, PowerBombPuzzle),
            // Charms
            CreatePlacement(PyromaniacCharm, PyromaniacCharmPuzzle),
            CreatePlacement(BombMasterCharm, BombMasterCharmPuzzle),
            CreatePlacement(ShellSalvagerCharm, ShellSalvagerCharmPuzzle)
        };
        
        ItemChangerMod.AddPlacements(placements);
    }

    private static AbstractPlacement CreatePlacement(string itemName, string locationName)
    {
        AbstractPlacement abstractPlacement = Finder.GetLocation(locationName).Wrap();
        abstractPlacement.Add(Finder.GetItem(itemName));
        return abstractPlacement;
    }

    #endregion
}
