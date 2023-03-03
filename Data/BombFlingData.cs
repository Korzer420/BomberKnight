using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.Data;

internal class BombFlingData
{
    #region Properties

    public FlingUtils.SelfConfig FlingConfig { get; set; }

    public Transform SourcePosition { get; set; }

    public Vector3 Offset { get; set; }

    #endregion
}
