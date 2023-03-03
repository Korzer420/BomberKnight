using BomberKnight.Enums;
using BomberKnight.ItemData;
using BomberKnight.ItemData.Locations;
using BomberKnight.ModInterop;
using BomberKnight.UnityComponents;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
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

namespace BomberKnight.BombElements;

/// <summary>
/// Controls all generic settings and actions regarding bombs.
/// </summary>
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
                _bomb.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
                Rigidbody2D rigidbody = _bomb.AddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 1f;
                rigidbody.mass = 100f;
                _bomb.AddComponent<CircleCollider2D>();
                _bomb.AddComponent<Bomb>().Type = BombType.GrassBomb;
                UnityEngine.Object.DontDestroyOnLoad(_bomb);
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
    public static int BombBagLevel { get; set; }

    /// <summary>
    /// Gets or sets if colorless hints should be added.
    /// </summary>
    public static bool ColorlessHelp { get; set; }

    /// <summary>
    /// Gets or sets if colorless hints should be added.
    /// </summary>
    public static bool Active { get; set; } = true;

    #endregion

    #region Event handler

    private static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        BombSpell.AddBombSpell();
        BombUI.UpdateBombPage();
        BombUI.UpdateTracker();
    }

    private static IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        try
        {
            if (!RandomizerInterop.PlayingRandomizer || RandomizerInterop.Settings.Enabled)
            {
                _bombQueue.RemoveAll(x => x == BombType.MiningBomb);
                if (_tracker != null)
                    UnityEngine.Object.Destroy(_tracker);
                if (_miningCounter != null)
                    GameManager.instance.StopCoroutine(_miningCounter);
                On.HeroController.Start -= HeroController_Start;
                On.HeroController.TakeDamage -= HeroController_TakeDamage;
                ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
                ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
                On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= IntCompare_OnEnter;
                On.KnightHatchling.Explode -= KnightHatchling_Explode;
                On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
                BombSpell.StopListening();
                BombDrop.StopListening();
                BombUI.StopListening();
                if (_shapeshiftRoutine != null)
                    GameManager.instance.StopCoroutine(_shapeshiftRoutine);
            }
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("An error occured while trying the return to main menu: " + exception, KorzUtils.Enums.LogType.Error, false);
        }
        yield return orig(self);
    }

    private static void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        if (!RandomizerInterop.PlayingRandomizer || Active)
            StartListening();
        orig(self);
        if (!RandomizerInterop.PlayingRandomizer || Active)
        {
            BombUI.UpdateTracker();
            BombUI.UpdateBombPage();
        }
    }

    private static void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        if (!RandomizerInterop.PlayingRandomizer || Active)
            StartListening();
        orig(self, permaDeath, bossRush);
        try
        {
            if (!RandomizerInterop.PlayingRandomizer || Active)
            {
                if (RandomizerInterop.Settings.Place == RandoType.Vanilla)
                    ItemManager.GeneratePlacements();
                if (ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(ItemManager.ShellSalvagerCharmPuzzle))
                    ShellSalvagerLocation.RollOrder(ItemManager.Seed);
                // Reset seed for shell salvager location. 
                ItemManager.Seed = -1;
            }
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("An error occured while trying to start a new game: " + exception, KorzUtils.Enums.LogType.Error, false);
        }
    }

    private static void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        if (go?.name == "BounceBomb Explosion")
            GameManager.instance.StartCoroutine(YeetKnight(go.transform.position, go.GetComponent<CircleCollider2D>().bounds.max));
        else
        {
            if (go?.name.StartsWith("Fake Explosion") == true)
                damageAmount = 0;
            else if (go?.name.Contains("Explosion") == true && CharmHelper.EquippedCharm("Pyromaniac"))
            {
                damageAmount = 0;
                if (UnityEngine.Random.Range(0, 4) == 0)
                    HeroController.instance.AddHealth(1);
                GameManager.instance.StartCoroutine((IEnumerator)HeroController.instance.GetType()
                    .GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(HeroController.instance, new object[] { CharmHelper.EquippedCharm(CharmRef.StalwartShell) ? 2.1f : 1.5f }));
            }
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

    private static void IntCompare_OnEnter(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
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

    private static void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (string.Equals(self.FsmName, "quake_floor"))
            {
                self.AddState(new FsmState(self.Fsm)
                {
                    Name = "Explosion Quake",
                    Actions = new FsmStateAction[]
                    {
                    self.GetState("Transient").Actions[0],
                    // Take this as an example why working with fsm can be absolute bs.
                    new Trigger2dEvent()
                    {
                        collideTag = new() { Value = "Wall Breaker", RawValue = "Wall Breaker"},
                        sendEvent = self.GetState("Transient").GetFirstActionOfType<Trigger2dEvent>().sendEvent,
                        storeCollider = new("None"),
                        collideLayer = new()
                    }
                    }
                });

                // To prevent the "not-destruction" of dive or bomb if the other one is active, we add a additional state that handles both.
                self.AddState(new FsmState(self.Fsm)
                {
                    Name = "Multiple Breaker",
                    Actions = new FsmStateAction[]
                    {
                    self.GetState("Transient").Actions[1],
                    new Trigger2dEvent()
                    {
                        collideTag = new() { Value = "Wall Breaker", RawValue = "Wall Breaker"},
                        sendEvent = self.GetState("Transient").GetFirstActionOfType<Trigger2dEvent>().sendEvent,
                        storeCollider = new("None"),
                        collideLayer = new()
                    }
                    }
                });

                FsmState explosionState = self.GetState("Explosion Quake");
                explosionState.AddTransition("VANISHED", "Solid");
                explosionState.AddTransition("DESTROY", "PD Bool?");
                explosionState.AddTransition("QUAKE FALL START", "Multiple Breaker");

                FsmState multipleBreak = self.GetState("Multiple Breaker");
                multipleBreak.AddTransition("DESTROY", "PD Bool?");
                multipleBreak.AddTransition("QUAKE FALL END", "Explosion Quake");
                multipleBreak.AddTransition("VANISHED", "Transient");

                self.GetState("Solid").AddTransition("BOMBED", explosionState);
                self.GetState("Solid").AddTransition("POWERBOMBED", "PD Bool?");
                self.GetState("Transient").AddTransition("BOMBED", multipleBreak);
                self.GetState("Transient").AddTransition("POWERBOMBED", "PD Bool?");
            }
            else if (string.Equals(self.FsmName, "Detect Quake"))
            {
                self.GetState("Detect").AddTransition("POWERBOMBED", "Quake Hit");
                self.GetState("Check Quake").AddTransition("POWERBOMBED", "Quake Hit");
            }
            else if (string.Equals(self.FsmName, "break_floor"))
                self.GetState("Idle")?.AddTransition("POWERBOMBED", self.GetState("PlayerData") != null ? "PlayerData" : "Break");
            else if (string.Equals(self.FsmName, "breakable_wall_v2"))
                self.GetState("Idle")?.AddTransition("POWERBOMBED", "PD Bool?");
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>($"Failed to modify ground {self.gameObject.name}: " + exception.ToString(), KorzUtils.Enums.LogType.Error);
        }
        orig(self);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Initializes the manager.
    /// </summary>
    internal static void Initialize()
    {
        try
        {
            On.UIManager.StartNewGame += UIManager_StartNewGame;
            On.UIManager.ContinueGame += UIManager_ContinueGame;
            On.UIManager.ReturnToMainMenu += UIManager_ReturnToMainMenu;

            foreach (BombType type in Enum.GetValues(typeof(BombType)))
                if (!AvailableBombs.ContainsKey(type))
                    AvailableBombs.Add(type, false);
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("Failed to initialize BombManager: " + exception, KorzUtils.Enums.LogType.Error, false);
        }
    }

    /// <summary>
    /// Set up all hooks for ingame modifications.
    /// </summary>
    private static void StartListening()
    {
        try
        {
            On.HeroController.Start += HeroController_Start;
            On.HeroController.TakeDamage += HeroController_TakeDamage;
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += IntCompare_OnEnter;
            On.KnightHatchling.Explode += KnightHatchling_Explode;
            On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
            ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            BombSpell.StartListening();
            BombDrop.StartListening();
            BombUI.StartListening();
            BombUI.UpdateBombPage();
            BombUI.UpdateTracker();
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("Error in setup: " + exception, KorzUtils.Enums.LogType.Error, false);
        }
    }

    /// <summary>
    /// Adds the bombs to the bomb queue.
    /// </summary>
    /// <param name="bombs">The bombs that should be added. Excessive ones get discarded.</param>
    public static void GiveBombs(IEnumerable<BombType> bombs)
    {
        try
        {
            int maxAmount = BombBagLevel * 10;
            if (BombQueue.Count >= maxAmount && !bombs.Any(x => x == BombType.MiningBomb))
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

            if (_bombQueue.Count > 0
                && _bombQueue[0] == BombType.GrassBomb && CharmHelper.EquippedCharm(CharmRef.ShapeOfUnn)
                && UnityEngine.Random.Range(0, 2) == 0)
                _shapeshiftRoutine ??= GameManager.instance.StartCoroutine(Shapeshift());
            else if (_shapeshiftRoutine != null)
                GameManager.instance.StopCoroutine(_shapeshiftRoutine);

            BombUI.UpdateBombPage();
            BombUI.UpdateTracker();
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("An error occured while trying to give bombs: " + exception, KorzUtils.Enums.LogType.Error, false);
        }
    }

    internal static void SetBombsSilent(List<BombType> bombTypes) => _bombQueue = bombTypes;

    /// <summary>
    /// Removes bombs out of the queue.
    /// </summary>
    /// <param name="amount">The amount that should be taken.</param>
    public static void TakeBombs(int amount = 1)
    {
        try
        {
            if (_bombQueue[0] == BombType.MiningBomb && _miningCounter != null)
            {
                GameManager.instance.StopCoroutine(_miningCounter);
                if (_tracker != null)
                    UnityEngine.Object.Destroy(_tracker);
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
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("An error ocurred while trying to take bobms away: " + exception, KorzUtils.Enums.LogType.Error, false);
        }
    }

    /// <summary>
    /// Controls the bounce effect of the bounce bombs.
    /// </summary>
    private static IEnumerator YeetKnight(Vector3 explosionPosition, Vector3 maxPoint)
    {
        float passedTime = 0f;
        float knockback = CalculateKnockback(explosionPosition, maxPoint);

        Rigidbody2D hero = HeroController.instance.GetComponent<Rigidbody2D>();
        // Trigger invincibility
        GameManager.instance.StartCoroutine((IEnumerator)HeroController.instance.GetType()
            .GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(HeroController.instance, new object[] { CharmHelper.EquippedCharm(CharmRef.StalwartShell) ? 4f : 2f }));
        float yeetTime = CharmHelper.EquippedCharm(BomberKnight.BombMasterCharm) ? 0.4f : 0.2f;
        while (passedTime < yeetTime)
        {
            hero.velocity = new(hero.velocity.x, HeroController.instance.JUMP_SPEED * knockback);
            yield return null;
            passedTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Calculates the knockback that should be given upon getting hit by a bounce bomb.
    /// </summary>
    /// <param name="explosionPosition"></param>
    /// <param name="maxPoint"></param>
    private static float CalculateKnockback(Vector3 explosionPosition, Vector3 maxPoint)
    {
        float explosionDistance = Vector3.Distance(explosionPosition, maxPoint);
        float knightDistance = Math.Min(explosionDistance, Vector3.Distance(explosionPosition, HeroController.instance.transform.position));
        return 1.5f + 1.5f * (100 / explosionDistance * knightDistance / 100);
    }

    /// <summary>
    /// Gets the color of the bomb/explosion based on the bomb type.
    /// </summary>
    /// <param name="bombType">The type of bomb which should be looked for.</param>
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

    /// <summary>
    /// Permanently changed the type of bomb if the current bomb is a grass bomb and shape of unn is equipped.
    /// </summary>
    private static IEnumerator Shapeshift()
    {
        List<BombType> bombTypes = AvailableBombs.Keys.Where(x => AvailableBombs[x]).ToList();
        if (bombTypes.Count <= 1)
            yield break;
        while (BombQueue.Count > 0 && CharmHelper.EquippedCharm(CharmRef.ShapeOfUnn))
        {
            _bombQueue[0] = bombTypes[UnityEngine.Random.Range(0, bombTypes.Count)];
            BombUI.UpdateTracker();
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Controls the ticking for the mining bomb (used for the bomb master charm)
    /// </summary>
    private static IEnumerator TickMiningBomb()
    {
        float time = 480f;
        if (_tracker == null)
        {
            GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
            GameObject hudCanvas = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas").gameObject;
            _tracker = UnityEngine.Object.Instantiate(prefab, hudCanvas.transform, true);
            _tracker.name = "Mining timer";
            _tracker.transform.localPosition = new(7.7818f, 0.5418f, 0);
            _tracker.transform.localScale = new(1.3824f, 1.3824f, 1.3824f);
            _tracker.GetComponent<DisplayItemAmount>().playerDataInt = _tracker.name;
            _tracker.GetComponent<DisplayItemAmount>().textObject.text = "";
            _tracker.GetComponent<DisplayItemAmount>().textObject.fontSize = 3;
            _tracker.GetComponent<DisplayItemAmount>().textObject.gameObject.name = "Counter";
            UnityEngine.Object.Destroy(_tracker.GetComponent<SpriteRenderer>());
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
        GameObject spawnedBomb = UnityEngine.Object.Instantiate(Bomb);
        spawnedBomb.GetComponent<Bomb>().Type = BombType.MiningBomb;
        spawnedBomb.GetComponent<Bomb>().CanExplode = true;
        spawnedBomb.transform.localPosition = HeroController.instance.transform.localPosition;
        spawnedBomb.transform.localScale = new(2f, 2f, 1f);
        spawnedBomb.name = "Bomb";
        spawnedBomb.SetActive(true);
        if (_tracker != null)
            UnityEngine.Object.Destroy(_tracker);
    }

    #endregion
}
