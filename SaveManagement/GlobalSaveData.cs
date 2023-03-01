namespace BomberKnight.SaveManagement;

/// <summary>
/// Contains data which is used across all save files.
/// </summary>
public class GlobalSaveData
{
    #region Properties

    /// <summary>
    /// Gets or sets the flag that indicates if additional info for color blind people should be displayed.
    /// </summary>
    public bool ColorlessHelp { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates if bombs should be cast with the cast button. Otherwise the quick cast button is used.
    /// </summary>
    public bool BombFromCast { get; set; }

    #endregion
}
