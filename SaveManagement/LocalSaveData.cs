using BomberKnight.Enums;
using System.Collections.Generic;

namespace BomberKnight.SaveManagement;

/// <summary>
/// Stores information about the current save file progress.
/// </summary>
public class LocalSaveData
{
    #region Properties

    /// <summary>
    /// Gets or sets the states of each available bomb types.
    /// </summary>
    public Dictionary<BombType, bool> AvailableBombTypes { get; set; }

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

    #endregion
}
