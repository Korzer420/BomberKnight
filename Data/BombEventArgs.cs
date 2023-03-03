using BomberKnight.Enums;
using UnityEngine;

namespace BomberKnight.Data;

/// <summary>
/// Contains event data for a bomb event.
/// </summary>
public class BombEventArgs
{
    #region Constructor

    public BombEventArgs(BombType bombType, Vector3 position)
    {
        Type = bombType;
        Position = position;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the type of the bomb.
    /// </summary>
    public BombType Type { get; }

    /// <summary>
    /// Gets the position of the object.
    /// </summary>
    public Vector3 Position { get; set; }

    #endregion
}
