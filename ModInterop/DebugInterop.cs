using BomberKnight.Enums;
using DebugMod;
using System.Collections.Generic;
using System.Linq;

namespace BomberKnight.ModInterop;

public static class DebugInterop
{
    #region Control

    internal static void Initialize() => DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));

    #endregion

    #region Methods

    /// <summary>
    /// Adds a bomb bag. Can exceed the normal limit.
    /// </summary>
    [BindableMethod(name = "Add Bomb Bag", category = "BomberKnight")]
    public static void AddBombBag()
    {
        BombManager.BombBagLevel++;
        if (BombManager.BombBagLevel > 0)
        { 
            BombUI.UpdateTracker();
            BombUI.UpdateBombPage();
        }
        Console.AddLine("Adding bomb bag");
    }

    /// <summary>
    /// Remove a bomb bag. Excessive bombs will be disposed.
    /// </summary>
    [BindableMethod(name = "Remove Bomb Bag", category = "BomberKnight")]
    public static void RemoveBombBag()
    {
        if (BombManager.BombBagLevel > 0)
        {
            BombManager.BombBagLevel--;
            if (BombManager.BombQueue.Count > BombManager.BombBagLevel * 10)
                BombManager.TakeBombs(BombManager.BombQueue.Count - BombManager.BombBagLevel * 10);
            Console.AddLine("Removing bomb bag");
            BombUI.UpdateBombPage();
        }
        else
            Console.AddLine("Couldn't remove bomb bag. Already at 0.");
    }

    /// <summary>
    /// Adds random useable bombs to the bomb bag (excluding power and quest bombs).
    /// </summary>
    [BindableMethod(name = "Fill Bomb Bag (Random)", category = "BomberKnight")]

    public static void FillBombBagRandom()
    {
        if (BombManager.BombQueue.Count >= BombManager.BombBagLevel * 10)
        {
            Console.AddLine("Bomb bag is already full");
            return;
        }
        List<BombType> availableBombTypes = BombManager.AvailableBombs.Where(x => x.Value && x.Key != BombType.PowerBomb && x.Key != BombType.MiningBomb).Select(x => x.Key).ToList();
        if (!availableBombTypes.Any())
        {
            Console.AddLine("Couldn't find bomb type to add.");
            return;
        }
        List<BombType> bombsToAdd = new();
        for (int i = BombManager.BombQueue.Count; i < BombManager.BombBagLevel * 10; i++)
            bombsToAdd.Add(availableBombTypes[UnityEngine.Random.Range(0, availableBombTypes.Count)]);
        
        BombManager.GiveBombs(bombsToAdd);
        Console.AddLine($"Filled bomb bag with random bombs.");
    }

    /// <summary>
    /// Removes all bombs from the bomb bag.
    /// </summary>
    [BindableMethod(name = "Clear Bomb Bag", category = "BomberKnight")]
    public static void ClearBombBag()
    {
        Console.AddLine($"Removed {BombManager.BombQueue.Count} bombs.");
        BombManager.TakeBombs(BombManager.BombQueue.Count);
    }

    /// <summary>
    /// Fills the bag with grass bombs.
    /// </summary>
    [BindableMethod(name = "Fill Bomb Bag (Grass)", category = "BomberKnight")]
    public static void FillGrassBombs() => FillBombBag(BombType.GrassBomb);

    /// <summary>
    /// Fills the bag with spore bombs.
    /// </summary>
    [BindableMethod(name = "Fill Bomb Bag (Spore)", category = "BomberKnight")]
    public static void FillSporeBombs() => FillBombBag(BombType.SporeBomb);

    /// <summary>
    /// Fills the bag with grass bombs.
    /// </summary>
    [BindableMethod(name = "Fill Bomb Bag (Bounce)", category = "BomberKnight")]
    public static void FillBounceBombs() => FillBombBag(BombType.BounceBomb);

    /// <summary>
    /// Fills the bag with gold bombs.
    /// </summary>
    [BindableMethod(name = "Fill Bomb Bag (Gold)", category = "BomberKnight")]
    public static void FillGoldBombs() => FillBombBag(BombType.GoldBomb);

    /// <summary>
    /// Fills the bag with grass bombs.
    /// </summary>
    [BindableMethod(name = "Fill Bomb Bag (Echo)", category = "BomberKnight")]
    public static void FillEchoBombs() => FillBombBag(BombType.EchoBomb);

    /// <summary>
    /// Unlocks all bomb types.
    /// </summary>
    [BindableMethod(name = "Unlock bombs", category = "BomberKnight")]
    public static void UnlockAllBombs()
    {
        foreach (BombType type in BombManager.AvailableBombs.Keys.ToArray())
            BombManager.AvailableBombs[type] = true;
        Console.AddLine("Unlocked all bomb types.");
    }

    /// <summary>
    /// Locks all bomb types.
    /// </summary>
    [BindableMethod(name = "Lock bombs", category = "BomberKnight")]
    public static void LockAllBombs()
    {
        foreach (BombType type in BombManager.AvailableBombs.Keys.ToArray())
            BombManager.AvailableBombs[type] = false;
        Console.AddLine("Locked all bomb types.");
    }

    #endregion

    #region Private Methods

    private static void FillBombBag(BombType bombType)
    {
        if (BombManager.BombQueue.Count >= BombManager.BombBagLevel * 10)
        {
            Console.AddLine("Bomb bag is already full");
            return;
        }
        List<BombType> bombTypes = Enumerable.Range(0, BombManager.BombBagLevel * 10 - BombManager.BombQueue.Count).Select(x => BombType.GrassBomb).ToList();
        BombManager.GiveBombs(bombTypes);
        Console.AddLine($"Filled bomb bag with {bombType.ToString().Substring(0, bombType.ToString().Length - 4)} bombs");
    }

    #endregion
}
