using BomberKnight.Enums;
using BomberKnight.ItemData;
using System.Collections.Generic;

namespace BomberKnight.SaveManagement;

/// <summary>
/// Stores information about the current save file progress.
/// </summary>
public class LocalSaveData
{
    #region Properties

    /// <summary>
    /// Gets the flag that indicates if the mod is used (only used in rando cases).
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the states of each available bomb types.
    /// </summary>
    public Dictionary<BombType, bool> AvailableBombTypes { get; set; }

    /// <summary>
    /// Gets or sets the states of the custom charms.
    /// </summary>
    public List<CharmData> CharmData { get; set; }

    /// <summary>
    /// Gets or sets the current level of the bomb bag.
    /// </summary>
    public int BombBagLevel { get; set; }

    /// <summary>
    /// Gets or sets the knight/chest order for the bomb scraper charm location.
    /// </summary>
    public List<string> KnightOrder { get; set; }

    /// <summary>
    /// Gets or sets the current bomb inventory.
    /// </summary>
    public List<BombType> Inventory { get; set; }

    /// <summary>
    /// Gets or sets the current inventory of the shade.
    /// </summary>
    public List<BombType> ShadeInventory { get; set; }

    #endregion
}
