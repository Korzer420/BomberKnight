using BomberKnight.Enums;
using BomberKnight.EventArgs;
using BomberKnight.UnityComponents;
using KorzUtils.Enums;
using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BomberKnight;

/// <summary>
/// Provides function to evaluate everything about bomb drops. (The consumable, which can increase the bomb amount, not the drop of the bomb itself)
/// </summary>
public static class BombDrop
{
    #region Event data

    public delegate DropData DropCalculation(DropEventArgs dropEventArgs);

    /// <summary>
    /// Fired when the bomb drop should be spawned. Drop data can be modified.
    /// </summary>
    public static event DropCalculation OnDrop;

    #endregion

    #region Members

    private static float _commonDropChance = 0.6f;
    private static float _uncommonDropChance = 0.3f;
    private static float _rareDropChance = 0.1f;
    private static int _bombCounter = 0;
    private static FieldInfo[] _geoInfo = new FieldInfo[3];

    #endregion

    #region Properties

    /// <summary>
    /// Gets all bombs which are considered common. 
    /// <para> The chance of a specific common bomb is 60% divided by the amount of common bomb types.</para>
    /// </summary>
    public static List<BombType> CommonBombs { get; } = new() { BombType.GrassBomb };

    /// <summary>
    /// Gets all bombs which are considered uncommon. 
    /// <para> The chance of a specific uncommon bomb is 30% divided by the amount of uncommon bomb types.</para>
    /// </summary>
    public static List<BombType> UncommonBombs { get; } = new() { BombType.SporeBomb, BombType.BounceBomb };

    /// <summary>
    /// Gets all bombs which are considered rare. 
    /// <para> The chance of a specific rare bomb is 10% divided by the amount of rare bomb types.</para>
    /// </summary>
    public static List<BombType> RareBombs { get; } = new() { BombType.GoldBomb, BombType.EchoBomb };

    #endregion

    #region Event handler

    private static void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        orig(self, go, damageSide, damageAmount, hazardType);
        if (damageAmount > 0)
            _bombCounter = 0;
    }

    private static void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        if (attackType != AttackTypes.RuinsWater && !self.gameObject.name.Equals("Hollow Knight Boss"))
        {
            _bombCounter++;
            if (_bombCounter >= 3 && self.GetComponent<NoBomb>() == null && (Convert.ToInt32(_geoInfo[0].GetValue(self)) > 0 ||
                Convert.ToInt32(_geoInfo[1].GetValue(self)) > 0 || Convert.ToInt32(_geoInfo[2].GetValue(self)) > 0))
            {
                _bombCounter = 0;
                DropBombs(self.gameObject);
                self.SetGeoLarge(0);
                self.SetGeoMedium(0);
                self.SetGeoSmall(0);
            }
        }
        orig(self, attackDirection, attackType, ignoreEvasion);
    }

    #endregion

    #region Drop Rates

    /// <summary>
    /// Sets the drop rates to default.
    /// </summary>
    public static void ResetDropRate()
    {
        _commonDropChance = 0.6f;
        _uncommonDropChance = 0.3f;
        _rareDropChance = 0.1f;
    }

    /// <summary>
    /// Changes the chances for each drop CATEGORY from being selected.
    /// <para>Note that the sum of the parameter has to be 0, otherwise the change is ignored. Like: AdjustDropRate(0.15f, 0.15f, -0.3f)</para>
    /// </summary>
    /// <param name="commonChange"></param>
    /// <param name="uncommonChange"></param>
    /// <param name="rareChange"></param>
    /// <returns>If the drop rate got set correctly</returns>
    public static bool AdjustDropRate(float commonChange, float uncommonChange, float rareChange)
    {
        // Make sure that we don't exceed 100%
        if (commonChange + uncommonChange + rareChange != 0)
            return false;

        _commonDropChance += commonChange;
        _uncommonDropChance += uncommonChange;
        _rareDropChance += rareChange;

        return true;
    }

    #endregion

    internal static void Initialize()
    {
        _geoInfo[0] = typeof(HealthManager).GetField("smallGeoDrops", BindingFlags.Instance | BindingFlags.NonPublic);
        _geoInfo[1] = typeof(HealthManager).GetField("mediumGeoDrops", BindingFlags.Instance | BindingFlags.NonPublic);
        _geoInfo[2] = typeof(HealthManager).GetField("largeGeoDrops", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    internal static void StartListening()
    {
        On.HealthManager.OnEnable += HealthManager_OnEnable;
        On.HealthManager.Die += HealthManager_Die;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
    }

    private static void HealthManager_OnEnable(On.HealthManager.orig_OnEnable orig, HealthManager self)
    {
        orig(self);
        if (self.gameObject.name == "Giant Fly" || self.gameObject.name == "Giant Buzzer" 
            || self.gameObject.name == "Mega Moss Charger" || self.hp >= 200)
            self.gameObject.AddComponent<NoBomb>();
    }

    internal static void StopListening()
    {
        On.HealthManager.OnEnable -= HealthManager_OnEnable;
        On.HealthManager.Die -= HealthManager_Die;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
    }

    /// <summary>
    /// Generate a list of the bomb drop.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    private static List<BombType> RollDrop(GameObject source)
    {
        List<(BombType, float)> chanceList = source.gameObject.name.Contains("Mushroom") && BombManager.AvailableBombs[BombType.SporeBomb]
            ? new() { (BombType.SporeBomb, 1f) }
            : GetDropChances();

        List<BombType> rolledBombs = new();
        int amount = 1;

        int rolled = UnityEngine.Random.Range(1, 11);
        if (rolled <= 4)
            amount = 2;
        else if (rolled <= 7)
            amount = 3;
        else if (rolled == 10)
            amount = 4;

        // Charm doubles the dropped amount.
        if (CharmHelper.EquippedCharm("ShellSalvager"))
            amount *= 2;

        for (int i = 0; i < amount; i++)
        {
            // Some charms take priority on the drop.
            int initialRoll = UnityEngine.Random.Range(0, 10);
            if (initialRoll <= 3 && CharmHelper.EquippedCharm(CharmRef.HeavyBlow) && BombManager.AvailableBombs.ContainsKey(BombType.BounceBomb))
                rolledBombs.Add(BombType.BounceBomb);
            else if (initialRoll > 3 && initialRoll <= (CharmHelper.EquippedCharm(CharmRef.UnbreakableGreed) ? 5 : 4)
                && CharmHelper.EquippedCharm(CharmRef.FragileGreed) && BombManager.AvailableBombs.ContainsKey(BombType.GoldBomb))
                rolledBombs.Add(BombType.GoldBomb);
            else if (initialRoll > 5 && CharmHelper.EquippedCharm(CharmRef.Sporeshroom) && BombManager.AvailableBombs.ContainsKey(BombType.SporeBomb))
                rolledBombs.Add(BombType.SporeBomb);
            else
            {
                float rolledType = UnityEngine.Random.Range(0f, 1f);
                for (int j = 0; j < chanceList.Count; j++)
                {
                    if (rolledType < chanceList[j].Item2)
                    {
                        rolledBombs.Add(chanceList[j].Item1);
                        break;
                    }
                    rolledType -= chanceList[j].Item2;
                }
            }
        }
        return rolledBombs;
    }

    private static List<(BombType, float)> GetDropChances()
    {
        BombType[] unavailableBombTypes = BombManager.AvailableBombs.Where(x => !x.Value).Select(x => x.Key).ToArray();

        // Set up base chances based on the amount of viable bombs in the same quality.
        Dictionary<BombType, float> chances = ((BombType[])Enum.GetValues(typeof(BombType)))
            .ToDictionary(x => x, x =>
            {
                if (unavailableBombTypes.Contains(x))
                    return 0f;
                if (CommonBombs.Contains(x))
                    return _commonDropChance / CommonBombs.Except(unavailableBombTypes).Count();
                else if (UncommonBombs.Contains(x))
                    return _uncommonDropChance / UncommonBombs.Except(unavailableBombTypes).Count();
                else if (RareBombs.Contains(x))
                    return _rareDropChance / RareBombs.Except(unavailableBombTypes).Count();
                return 0f;
            });

        // Remove undroppable bombs.
        foreach (BombType type in (BombType[])Enum.GetValues(typeof(BombType)))
            if (chances[type] == 0f)
                chances.Remove(type);

        int commonBombAmount = CommonBombs.Except(unavailableBombTypes).Count();
        int uncommonBombAmount = UncommonBombs.Except(unavailableBombTypes).Count();
        int rareBombAmount = RareBombs.Except(unavailableBombTypes).Count();

        // Adjust drop rates for each missing type.
        (bool, bool, bool) groupAvailable = new(commonBombAmount > 0, uncommonBombAmount > 0, rareBombAmount > 0);
        switch (groupAvailable)
        {
            // Only common
            case (true, false, false):
                foreach (BombType bombType in chances.Keys.ToList())
                    chances[bombType] += (_uncommonDropChance + _rareDropChance) / commonBombAmount;
                break;
            // Only uncommon
            case (false, true, false):
                foreach (BombType bombType in chances.Keys.ToList())
                    chances[bombType] += (_commonDropChance + _rareDropChance) / uncommonBombAmount;
                break;
            // Common and uncommon
            case (true, true, false):
                foreach (BombType bombType in chances.Keys.ToList())
                    chances[bombType] += _rareDropChance / (uncommonBombAmount + commonBombAmount);
                break;
            // Common and rare
            case (true, false, true):
                foreach (BombType bombType in chances.Keys.ToList())
                    chances[bombType] += _uncommonDropChance / (commonBombAmount + rareBombAmount);
                break;
            // Uncommon and rare
            case (false, true, true):
                foreach (BombType bombType in chances.Keys.ToList())
                    chances[bombType] += _commonDropChance / (uncommonBombAmount + rareBombAmount);
                break;
            // Only rare
            case (false, false, true):
                foreach (BombType bombType in chances.Keys.ToList())
                    chances[bombType] += (_uncommonDropChance + _commonDropChance) / rareBombAmount;
                break;
            // Backup in case none bomb is available (which should never be the case)
            case (false, false, false):
                return new() { (BombType.GrassBomb, 1f) };
            // All
            default:
                break;
        }

        List<(BombType, float)> chanceList = new();
        foreach (BombType bombType in chances.Keys)
            chanceList.Add(new(bombType, chances[bombType]));
        return chanceList;
    }

    /// <summary>
    /// Calculates and drop some bomb pick ups.
    /// </summary>
    /// <param name="source"></param>
    public static void DropBombs(GameObject source)
    {
        List<BombType> bombTypes = RollDrop(source);
        DropData dropData = new(source.transform.position, new(0f, 2f), bombTypes);
        DropData modifiedDropData = OnDrop?.Invoke(new(dropData));
        if (modifiedDropData != null)
            dropData = modifiedDropData;
        LogHelper.Write<BomberKnight>("Spawn bomb", KorzUtils.Enums.LogType.Debug);
        GameObject dropObject = new("Bomb drop");
        dropObject.AddComponent<Rigidbody2D>().mass = 10f;
        dropObject.AddComponent<CircleCollider2D>();
        dropObject.AddComponent<BombPickup>().Bombs = dropData.Drops;
        dropObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("BombSprite");
        dropObject.SetActive(true);
        dropObject.transform.localScale = new(1.2f, 1.2f);
        dropObject.transform.position = source.transform.position;
        dropObject.GetComponent<Rigidbody2D>().AddForce(dropData.FlingForce);
    }
}
