using BomberKnight.Enums;
using BomberKnight.Helper;
using HutongGames.PlayMaker;
using InControl;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.UnityComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight;

public static class BombManager
{
    #region Members

    private static GameObject _bomb;

    private static List<BombType> _bombQueue = new();

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
                _bomb.SetActive(false);
                _bomb.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite("BombSprite");
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

    public static int BombBagLevel { get; set; } = 1;

    #endregion

    #region Event handler

    private static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        BombSpell.AddBombSpell();
    }

    private static System.Collections.IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        On.HeroController.Start -= HeroController_Start;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
        BombSpell.StopListening();
        yield return orig(self);
    }

    private static void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        On.HeroController.Start += HeroController_Start;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        BombSpell.StartListening();
        orig(self);
        GiveBombs(new List<BombType>() { BombType.BounceBomb, BombType.EchoBomb });
    }

    private static void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        On.HeroController.Start += HeroController_Start;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        BombSpell.StartListening();
        orig(self, permaDeath, bossRush);
        GiveBombs(new List<BombType>() { BombType.SporeBomb, BombType.EchoBomb });
    }

    private static void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        if (go.name == "BounceBomb Explosion")
            GameManager.instance.StartCoroutine(YeetKnight(go.transform.position, go.GetComponent<CircleCollider2D>().bounds.max));
        else
            orig(self, go, damageSide, damageAmount, hazardType);
    }

    #endregion

    #region Methods

    internal static void Initialize()
    {
        On.UIManager.StartNewGame += UIManager_StartNewGame;
        On.UIManager.ContinueGame += UIManager_ContinueGame;
        On.UIManager.ReturnToMainMenu += UIManager_ReturnToMainMenu;
    }

    /// <summary>
    /// Adds the bombs to the bomb queue.
    /// </summary>
    /// <param name="bombs">The bombs that should be added. Excessive ones get discarded.</param>
    public static void GiveBombs(IEnumerable<BombType> bombs)
    {
        int maxAmount = BombBagLevel * 5;
        if (BombQueue.Count >= maxAmount)
            return;

        // Discard all bombs that exceed the limit.
        if (BombQueue.Count + bombs.Count() > maxAmount)
            _bombQueue.AddRange(bombs.Take(BombQueue.Count + bombs.Count() - maxAmount));
        else
            _bombQueue.AddRange(bombs);
    }

    /// <summary>
    /// Removes bombs out of the queue.
    /// </summary>
    /// <param name="amount">The amount that should be taken.</param>
    public static void TakeBombs(int amount = 1)
    {
        _bombQueue = _bombQueue.Skip(amount).ToList();

        // To do: Update UI
    }

    private static IEnumerator YeetKnight(Vector3 explosionPosition, Vector3 maxPoint)
    {
        float passedTime = 0f;
        float knockback =  CalculateKnockback(explosionPosition, maxPoint);
        
        Rigidbody2D hero = HeroController.instance.GetComponent<Rigidbody2D>();
        // Trigger invincibility
        GameManager.instance.StartCoroutine((IEnumerator)HeroController.instance.GetType()
            .GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(HeroController.instance, new object[] { 3f }));
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

    #endregion
}
