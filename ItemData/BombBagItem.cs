using BomberKnight.Enums;
using ItemChanger;
using System.Collections.Generic;
using System.Linq;

namespace BomberKnight.ItemData;

internal class BombBagItem : AbstractItem
{
    public override void GiveImmediate(GiveInfo info)
    {
        // Auto unlock grass bombs.
        BombManager.AvailableBombs[BombType.GrassBomb] = true;
        BombManager.BombBagLevel++;

        // Auto fill the bomb bag with rando available bombs.
        List<BombType> availableBombs = BombManager.AvailableBombs.Keys.Where(x => x != BombType.PowerBomb && BombManager.AvailableBombs[x]).ToList();
        List<BombType> selectedBombs = new();

        for (int i = BombManager.BombQueue.Count; i < BombManager.BombBagLevel * 10; i++)
            selectedBombs.Add(availableBombs[UnityEngine.Random.Range(0, availableBombs.Count)]);
        BombManager.GiveBombs(selectedBombs);
    }
}
