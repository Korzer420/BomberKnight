using BomberKnight.Enums;
using BomberKnight.ItemData;
using BomberKnight.ItemData.Locations;
using BomberKnight.Resources;
using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.UIDefs;
using KorzUtils.Enums;
using KorzUtils.Helper;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using static Mono.Security.X509.X520;

namespace BomberKnight;

public static class BombManager
{
    #region Members

    private static GameObject _bomb;

    private static GameObject _tracker;

    private static List<BombType> _bombQueue = new();

    private static Coroutine _shapeshiftRoutine;

    private static Coroutine _miningCounter;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the prefab bomb object.
    /// </summary>
    public static GameObject Bomb
    {
        get
        {
            if (_bomb == null)
            {
                _bomb = new("Bomb");
                _bomb.layer = 12;
                _bomb.SetActive(false);
                _bomb.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("BombSprite");
                Rigidbody2D rigidbody = _bomb.AddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 1f;
                rigidbody.mass = 100f;
                _bomb.AddComponent<CircleCollider2D>();
                _bomb.AddComponent<Bomb>().Type = BombType.GrassBomb;
                GameObject.DontDestroyOnLoad(_bomb);
            }
            return _bomb;
        }
    }

    /// <summary>
    /// Gets the bomb list in order the player can cast them.
    /// </summary>
    public static IReadOnlyList<BombType> BombQueue => _bombQueue.AsReadOnly();

    /// <summary>
    /// Gets or sets the bombs that the player can cast.
    /// </summary>
    public static Dictionary<BombType, bool> AvailableBombs { get; set; } = new();

    /// <summary>
    /// Gets or sets the current level of the bomb bag. For each level the max amount is increased by 10.
    /// </summary>
    public static int BombBagLevel { get; set; } = 1;

    #endregion

    #region Event handler

    private static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        BombSpell.AddBombSpell();
    }

    private static IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        _bombQueue.RemoveAll(x => x == BombType.MiningBomb);
        if (_tracker != null)
            GameObject.Destroy(_tracker);
        if (_miningCounter != null)
            GameManager.instance.StopCoroutine(_miningCounter);
        On.HeroController.Start -= HeroController_Start;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= IntCompare_OnEnter;
        On.KnightHatchling.Explode -= KnightHatchling_Explode;
        BombSpell.StopListening();
        BombDrop.StopListening();
        BombUI.StopListening();
        if (_shapeshiftRoutine != null)
            GameManager.instance.StopCoroutine(_shapeshiftRoutine);
        yield return orig(self);
    }

    private static void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        StartListening();
        orig(self);
        GiveBombs(new List<BombType>() { BombType.SporeBomb, BombType.EchoBomb, BombType.GrassBomb, BombType.GoldBomb, BombType.BounceBomb });
    }

    private static void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        StartListening();
        orig(self, permaDeath, bossRush);
        ItemChangerMod.CreateSettingsProfile(false);
        BombScraperCharmLocation location = new BombScraperCharmLocation()
        {
            name = "Greenpath_Bag",
            sceneName = "Ruins1_06"
        };
        location.RollOrder();
        AbstractPlacement greenPath = location.Wrap();
        greenPath.Add(new BombBagItem()
        {
            name = "bombBag",
            UIDef = new BigUIDef()
            {
                bigSprite = new WrappedSprite("BombBag"),
                name = new BoxedString("Bomb Bag"),
                descOne = new BoxedString(InventoryText.BombBag_ItemScreen_Desc1),
                descTwo = new BoxedString(InventoryText.BombBag_ItemScreen_Desc2),
                shopDesc = new BoxedString("Please buy this quickly. Maybe then this \"Michael Bay\" will finally leave me alone.")
            }
        });
        ItemChangerMod.AddPlacements(new List<AbstractPlacement>()
        {
            greenPath
        });
        GiveBombs(new List<BombType>() { BombType.SporeBomb, BombType.EchoBomb, BombType.GrassBomb, BombType.GoldBomb, BombType.BounceBomb });
    }

    private static void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        if (go?.name == "BounceBomb Explosion")
            GameManager.instance.StartCoroutine(YeetKnight(go.transform.position, go.GetComponent<CircleCollider2D>().bounds.max));
        else
        {
            if (go?.name.StartsWith("Fake Explosion") == true)
                damageAmount = 0;
            orig(self, go, damageSide, damageAmount, hazardType);
        }
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == "HasBombBag")
            return BombBagLevel > 0;
        return orig;
    }

    private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "Bomber_Knight")
            return "Bomber Knight";
        return orig;
    }

    private static void IntCompare_OnEnter(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, HutongGames.PlayMaker.Actions.IntCompare self)
    {
        if (self.IsCorrectContext("Control", "Blocker Shield", "Blocker Hit") && BombBagLevel > 0
            && BombQueue.Count > 0 && UnityEngine.Random.Range(0, 5) == 0)
        {
            GameObject bomb = BombSpell.SpawnBomb(false, false, _bombQueue.Last());
            _bombQueue.RemoveAt(_bombQueue.Count - 1);
            BombUI.UpdateBombPage();
            BombUI.UpdateTracker();
            bomb.GetComponent<Bomb>().CanExplode = true;
        }
        orig(self);
    }

    private static IEnumerator KnightHatchling_Explode(On.KnightHatchling.orig_Explode orig, KnightHatchling self)
    {
        if (BombBagLevel > 0 && UnityEngine.Random.Range(0, 100) <= 1)
            BombDrop.DropBombs(self.gameObject);
        yield return orig(self);
    }

    #endregion

    #region Methods

    internal static void Initialize()
    {
        On.UIManager.StartNewGame += UIManager_StartNewGame;
        On.UIManager.ContinueGame += UIManager_ContinueGame;
        On.UIManager.ReturnToMainMenu += UIManager_ReturnToMainMenu;

        foreach (BombType type in Enum.GetValues(typeof(BombType)))
            if (!AvailableBombs.ContainsKey(type))
                AvailableBombs.Add(type, true);
    }

    private static void StartListening()
    {
        try
        {
            On.HeroController.Start += HeroController_Start;
            On.HeroController.TakeDamage += HeroController_TakeDamage;
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += IntCompare_OnEnter;
            On.KnightHatchling.Explode += KnightHatchling_Explode;
            ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            BombSpell.StartListening();
            BombDrop.StartListening();
            BombUI.StartListening();
            BombUI.UpdateBombPage();
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("Error trying to set up modifications: " + exception.ToString(), KorzUtils.Enums.LogType.Error);
        }
    }

    /// <summary>
    /// Adds the bombs to the bomb queue.
    /// </summary>
    /// <param name="bombs">The bombs that should be added. Excessive ones get discarded.</param>
    public static void GiveBombs(IEnumerable<BombType> bombs)
    {
        int maxAmount = BombBagLevel * 10;
        if (BombQueue.Count >= maxAmount)
            return;

        if (bombs.Any(x => x == BombType.MiningBomb))
        {
            if (BombQueue.Count == maxAmount)
                _bombQueue.RemoveAt(0);
            _bombQueue.Insert(0, BombType.MiningBomb);
            _miningCounter = GameManager.instance.StartCoroutine(TickMiningBomb());
        }
        else
        {
            // Discard all bombs that exceed the limit.
            if (BombQueue.Count + bombs.Count() > maxAmount)
                _bombQueue.AddRange(bombs.Take(BombQueue.Count + bombs.Count() - maxAmount));
            else
                _bombQueue.AddRange(bombs);
        }

        BombUI.Tracker.GetComponent<SpriteRenderer>().color = GetBombColor(BombQueue[0]);
        BombUI.Tracker.GetComponent<DisplayItemAmount>().textObject.text = _bombQueue.Count.ToString();

        if (_bombQueue.Count > 0
            && _bombQueue[0] == BombType.GrassBomb && CharmHelper.EquippedCharm(CharmRef.ShapeOfUnn)
            && UnityEngine.Random.Range(0, 2) == 0)
            _shapeshiftRoutine ??= GameManager.instance.StartCoroutine(Shapeshift());
        else if (_shapeshiftRoutine != null)
            GameManager.instance.StopCoroutine(_shapeshiftRoutine);

        BombUI.UpdateBombPage();
    }

    /// <summary>
    /// Removes bombs out of the queue.
    /// </summary>
    /// <param name="amount">The amount that should be taken.</param>
    public static void TakeBombs(int amount = 1)
    {
        if (_bombQueue[0] == BombType.MiningBomb && _miningCounter != null)
        { 
            GameManager.instance.StopCoroutine(_miningCounter);
            if (_tracker != null)
                GameObject.Destroy(_tracker);
            amount = 1;
        }
        // Spell Twister grants a 25% chance that bombs are not taken.
        else if (CharmHelper.EquippedCharm(CharmRef.SpellTwister))
            for (int i = amount; i > 0; i--)
                if (UnityEngine.Random.Range(0, 4) == 0)
                    amount--;
        _bombQueue = _bombQueue.Skip(amount).ToList();
        
        if (_bombQueue.Count > 0 && _bombQueue[0] == BombType.GrassBomb
            && CharmHelper.EquippedCharm(CharmRef.ShapeOfUnn) && UnityEngine.Random.Range(0, 2) == 0)
            _shapeshiftRoutine ??= GameManager.instance.StartCoroutine(Shapeshift());
        else if (_shapeshiftRoutine != null)
            GameManager.instance.StopCoroutine(_shapeshiftRoutine);
        BombUI.UpdateTracker();
        BombUI.UpdateBombPage();
    }

    private static IEnumerator YeetKnight(Vector3 explosionPosition, Vector3 maxPoint)
    {
        float passedTime = 0f;
        float knockback = CalculateKnockback(explosionPosition, maxPoint);

        Rigidbody2D hero = HeroController.instance.GetComponent<Rigidbody2D>();
        // Trigger invincibility
        GameManager.instance.StartCoroutine((IEnumerator)HeroController.instance.GetType()
            .GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(HeroController.instance, new object[] { CharmHelper.EquippedCharm(CharmRef.StalwartShell) ? 4f : 2f }));
        while (passedTime < 0.5f)
        {
            hero.velocity = new(hero.velocity.x, HeroController.instance.JUMP_SPEED * knockback);
            yield return null;
            passedTime += Time.deltaTime;
        }
    }

    private static float CalculateKnockback(Vector3 explosionPosition, Vector3 maxPoint)
    {
        float explosionDistance = Vector3.Distance(explosionPosition, maxPoint);
        float knightDistance = Math.Min(explosionDistance, Vector3.Distance(explosionPosition, HeroController.instance.transform.position));

        // explosion 40f
        // knight 25f
        // 100 / 40 => 2.5
        // 2.5 * 25 = 62.5f
        return 1.5f + (1.5f * (100 / explosionDistance * knightDistance / 100));
    }

    public static Color GetBombColor(BombType bombType) => bombType switch
    {
        BombType.GrassBomb => Color.green,
        BombType.SporeBomb => new(1f, 0.4f, 0f),
        BombType.GoldBomb => Color.yellow,
        BombType.EchoBomb => new(1f, 0f, 1f),
        BombType.BounceBomb => Color.white,
        BombType.MiningBomb => Color.red,
        _ => Color.cyan
    };

    private static IEnumerator Shapeshift()
    {
        List<BombType> bombTypes = AvailableBombs.Keys.Where(x => AvailableBombs[x]).ToList();
        if (bombTypes.Count <= 1)
            yield break;
        float timer = 0f;
        while (BombQueue.Count > 0 && CharmHelper.EquippedCharm(CharmRef.ShapeOfUnn))
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer = 0f;
                _bombQueue[0] = bombTypes[UnityEngine.Random.Range(0, bombTypes.Count)];
                BombUI.UpdateTracker();
            }
            yield return null;
        }
    }

    private static IEnumerator TickMiningBomb()
    {
        float time = 480f;
        if (_tracker == null)
        {
            GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
            GameObject hudCanvas = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas").gameObject;
            _tracker = GameObject.Instantiate(prefab, hudCanvas.transform, true);
            _tracker.name = "Mining timer";
            _tracker.transform.localPosition = new(7.7818f, 0.5418f, 0);
            _tracker.transform.localScale = new(1.3824f, 1.3824f, 1.3824f);
            _tracker.GetComponent<DisplayItemAmount>().playerDataInt = _tracker.name;
            _tracker.GetComponent<DisplayItemAmount>().textObject.text = "";
            _tracker.GetComponent<DisplayItemAmount>().textObject.fontSize = 3;
            _tracker.GetComponent<DisplayItemAmount>().textObject.gameObject.name = "Counter";
            Component.Destroy(_tracker.GetComponent<SpriteRenderer>());
        }
        _tracker.SetActive(true);
        TextMeshPro textContainer = _tracker.GetComponent<DisplayItemAmount>().textObject;
        while (time > 0f)
        {
            time -= Time.deltaTime;
            if (time > 60f)
                textContainer.text = TimeSpan.FromSeconds(time).ToString(@"mm\:ss");
            else
                textContainer.text = "<color=#f77a31>" + TimeSpan.FromSeconds(time).ToString("ss") + "</color>";
            yield return null;
        }
        _bombQueue.Remove(BombType.MiningBomb);
        GameObject spawnedBomb = GameObject.Instantiate(Bomb);
        spawnedBomb.GetComponent<Bomb>().Type = BombType.MiningBomb;
        spawnedBomb.GetComponent<Bomb>().CanExplode = true;
        spawnedBomb.transform.localPosition = HeroController.instance.transform.localPosition;
        spawnedBomb.transform.localScale = new(2f, 2f, 1f);
        spawnedBomb.name = "Bomb";
        spawnedBomb.SetActive(true);
        if (_tracker != null)
            GameObject.Destroy(_tracker);
    }

    #endregion
}
