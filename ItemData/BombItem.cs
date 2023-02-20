using BomberKnight.Enums;
using ItemChanger;
using System.Collections.Generic;

namespace BomberKnight.ItemData;

internal class BombItem : AbstractItem
{
    public BombType Type { get; set; }

    public override void GiveImmediate(GiveInfo info)
    {
        BombManager.AvailableBombs[Type] = true;
        BombManager.GiveBombs(new List<BombType>() { Type, Type, Type });
    }
}