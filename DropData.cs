using BomberKnight.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight;

public class DropData
{
    #region Constructors

    public DropData(Vector3 location, Vector2 flingForce, List<BombType> droppedBombs)
    {
        Location = location;
        FlingForce = flingForce;
        Drops = droppedBombs;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The location where the bombs drop.
    /// </summary>
    public Vector3 Location { get; set; }

    /// <summary>
    /// The fling force that will be applied to the <see cref="Location"/>
    /// </summary>
    public Vector2 FlingForce { get; set; }

    /// <summary>
    /// Gets or sets all bombs which types which should be dropped.
    /// <para>If this contains more than 2, only the types of the first two are visible in the sprite.</para>
    /// </summary>
    public List<BombType> Drops { get; set; }

    #endregion
}
