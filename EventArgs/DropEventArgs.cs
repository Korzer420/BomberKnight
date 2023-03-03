using BomberKnight.Data;
using BomberKnight.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.EventArgs;

/// <summary>
/// Contains information about the bomb drop.
/// </summary>
public class DropEventArgs
{
    #region Constructors

    public DropEventArgs(DropData data)
    {
        Data = data;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the data which will be used for the drop.
    /// </summary>
    public DropData Data { get; set; }

    #endregion
}
