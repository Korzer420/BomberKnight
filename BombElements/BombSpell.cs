using BomberKnight.Enums;
using BomberKnight.UnityComponents;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KorzUtils.Enums;
using KorzUtils.Helper;
using System.Collections;
using System.Linq;
using UnityEngine;
using KU = KorzUtils.Enums;

namespace BomberKnight.BombElements;

/// <summary>
/// The modifications to allow the user to use bombs as their fourth "spell".
/// </summary>
internal static class BombSpell
{
    #region Members

    private static bool _listening;

    private static float _cooldown = 0f;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets whether the normal cast button should be used for bombs. Otherwise quick cast is used.
    /// </summary>
    public static bool UseCast { get; set; }

    #endregion

    #region Event handler

    private static void IntCompare_OnEnter(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
    {
        if (HeroController.instance.CanCast() && (self.IsCorrectContext("Spell Control", "Knight", "Can Cast? QC")
            || self.IsCorrectContext("Spell Control", "Knight", "Can Cast?"))
            && _cooldown <= 0f && BombManager.BombQueue.Any() && !InputHandler.Instance.inputActions.left.IsPressed
            && !InputHandler.Instance.inputActions.right.IsPressed && !InputHandler.Instance.inputActions.up.IsPressed
            && (self.State.Name == "Can Cast?" && UseCast || self.State.Name == "Can Cast? QC" && !UseCast))
            self.Fsm.FsmComponent.SendEvent("BOMB");
        orig(self);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Hooks the actions in the spell fsm to allow the user to access the spell.
    /// </summary>
    internal static void StartListening()
    {
        try
        {
            if (_listening)
                return;
            _listening = true;
            AddBombSpell();
            On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += IntCompare_OnEnter;
        }
        catch (System.Exception exception)
        {
            LogHelper.Write<BomberKnight>("Failed to setup bomb spell: " + exception, KU.LogType.Error, false, false);
        }
    }

    internal static void StopListening()
    {
        if (!_listening)
            return;
        _listening = false;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= IntCompare_OnEnter;
    }

    /// <summary>
    /// Modifies the hero to allow casting the bomb spell
    /// </summary>
    internal static void AddBombSpell()
    {
        try
        {
            if (HeroController.instance == null)
                return;
            LogHelper.Write<BomberKnight>("Modify spell control", KU.LogType.Debug);
            PlayMakerFSM fsm = HeroController.instance.spellControl;
            if (fsm.GetState("Normal bomb") is not null)
                return;
            FsmState placeBomb = new(fsm.Fsm)
            {
                Name = "Place bomb",
                Actions = new FsmStateAction[]
                {
                    new Lambda(() =>
                    {
                        // Fail save
                        if (!BombManager.BombQueue.Any())
                            fsm.SendEvent("CANCEL");

                        if (InputHandler.Instance.inputActions.down.IsPressed && CharmHelper.EquippedCharm(BomberKnight.BombMasterCharm))
                            Bomb.ActiveBombs.ForEach(x => x.CanExplode = true);

                        if (BombManager.AvailableBombs[BombType.PowerBomb] && InputHandler.Instance.inputActions.down.IsPressed
                            && BombManager.BombQueue.Count >= 3 && PlayerData.instance.GetInt("healthBlue") > 0)
                            fsm.SendEvent("POWERBOMB");
                        else
                            fsm.SendEvent("BOMB");
                    })
                }
            };

            FsmState normalBomb = new(fsm.Fsm)
            {
                Name = "Normal bomb",
                Actions = new FsmStateAction[]
                {
                    new Lambda(() => SpawnBomb())
                }
            };
            normalBomb.AddTransition("FINISHED", "Spell End");

            FsmState powerBomb = new(fsm.Fsm)
            {
                Name = "Power bomb",
                Actions = new FsmStateAction[]
                {
                new Lambda(() =>
                {
                    HeroController.instance.TakeHealth(1);
                    BombManager.TakeBombs(3);
                    SpawnBomb(true, false, BombType.PowerBomb);
                })
                }
            };
            powerBomb.AddTransition("FINISHED", "Spell End");

            placeBomb.AddTransition("BOMB", normalBomb);
            placeBomb.AddTransition("POWERBOMB", powerBomb);

            fsm.GetState("Can Cast? QC").AddTransition("CANCEL", "Inactive");
            fsm.GetState("Can Cast? QC").AddTransition("BOMB", placeBomb);

            fsm.GetState("Can Cast?").AddTransition("CANCEL", "Inactive");
            fsm.GetState("Can Cast?").AddTransition("BOMB", placeBomb);
        }
        catch (System.Exception exception)
        {
            LogHelper.Write<BomberKnight>("Failed to modify spell control: " + exception, KU.LogType.Error, false);
        }
    }

    internal static GameObject SpawnBomb(bool triggerCooldown = true, bool normalTake = true, BombType bombType = BombType.GrassBomb)
    {
        try
        {
            GameObject spawnedBomb = Object.Instantiate(BombManager.Bomb);
            spawnedBomb.transform.localPosition = HeroController.instance.transform.localPosition;
            spawnedBomb.transform.localScale = new(2f, 2f, 1f);
            spawnedBomb.name = "Bomb";
            if (normalTake)
            {
                spawnedBomb.GetComponent<Bomb>().Type = BombManager.BombQueue[0];
                BombManager.TakeBombs();
            }
            else
                spawnedBomb.GetComponent<Bomb>().Type = bombType;
            spawnedBomb.SetActive(true);
            if (triggerCooldown)
                GameManager.instance.StartCoroutine(Cooldown());
            return spawnedBomb;
        }
        catch (System.Exception exception)
        {
            throw exception;
        }
    }

    private static IEnumerator Cooldown()
    {
        _cooldown = CharmHelper.EquippedCharm(CharmRef.QuickFocus)
            ? 0.5f
            : 3f;
        while (_cooldown > 0f)
        {
            _cooldown -= Time.deltaTime;
            yield return new WaitUntil(() => !GameManager.instance.IsGamePaused());
        }
        _cooldown = 0f;
    }

    #endregion
}
