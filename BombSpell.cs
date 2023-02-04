using BomberKnight.Enums;
using BomberKnight.Helper;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.UnityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight;

/// <summary>
/// The modifications to allow the user to use bombs as their fourth "spell".
/// </summary>
internal static class BombSpell
{
    #region Members

    private static bool _listening;
    
    #endregion

    #region Event handler

    private static void ListenForUp_OnEnter(On.HutongGames.PlayMaker.Actions.ListenForUp.orig_OnEnter orig, ListenForUp self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "QC") && string.Equals(self.isPressed.Name, "SCREAM")
            && BombManager.BombQueue.Any())
        {
            if (InputHandler.Instance.inputActions.left.IsPressed)
                self.Fsm.FsmComponent.SendEvent("BOMB");
            else if (InputHandler.Instance.inputActions.right.IsPressed)
                self.Fsm.FsmComponent.SendEvent("POWERBOMB");
        }
        orig(self);
    }

    private static void BoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.BoolTest.orig_OnEnter orig, BoolTest self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell Choice") && string.Equals(self.isTrue.Name, "SCREAM")
            && BombManager.BombQueue.Any())
        {
            if (InputHandler.Instance.inputActions.left.IsPressed)
                self.Fsm.FsmComponent.SendEvent("BOMB");
            else if (InputHandler.Instance.inputActions.right.IsPressed)
                self.Fsm.FsmComponent.SendEvent("POWERBOMB");
        }
        orig(self);
    }

    private static void IntCompare_OnEnter(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
    {
        if (((self.IsCorrectContext("Spell Control", "Knight", "Can Cast? QC"))
            || self.IsCorrectContext("Spell Control", "Knight", "Can Cast?")) && BombManager.BombQueue.Any())
        {
            if (InputHandler.Instance.inputActions.right.IsPressed && PlayerData.instance.GetInt("healthBlue") > 0
                && BombManager.BombQueue.Count > 2 && BombManager.AvailableBombs.ContainsKey(BombType.PowerBomb))
                self.Fsm.FsmComponent.SendEvent("POWERBOMB");
            else if (InputHandler.Instance.inputActions.left.IsPressed && self.integer1.Value >= self.integer2.Value)
                self.Fsm.FsmComponent.SendEvent("BOMB");
        }
        orig(self);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Hooks the actions in the spell fsm to allow the user to access the spell.
    /// </summary>
    internal static void StartListening()
    {
        LogHelper.Write("Start listening", Helper.LogType.Debug);
        if (_listening)
            return;
        _listening = true;
        AddBombSpell();
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += IntCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter += BoolTest_OnEnter;
        On.HutongGames.PlayMaker.Actions.ListenForUp.OnEnter += ListenForUp_OnEnter;
    }

    internal static void StopListening()
    {
        LogHelper.Write("Stop listening", Helper.LogType.Debug);
        if (!_listening)
            return;
        _listening = false;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= IntCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter -= BoolTest_OnEnter;
        On.HutongGames.PlayMaker.Actions.ListenForUp.OnEnter -= ListenForUp_OnEnter;
    }

    /// <summary>
    /// Modifies the hero to allow casting the bomb spell
    /// </summary>
    internal static void AddBombSpell()
    {
        if (HeroController.instance == null)
            return;
        LogHelper.Write("Modify spell control");
        PlayMakerFSM fsm = HeroController.instance.spellControl;
        if (fsm.GetState("Normal bomb") is not null)
            return;
        FsmState normalBomb = new(fsm.Fsm)
        {
            Name = "Normal bomb",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    HeroController.instance.TakeMP(fsm.FsmVariables.FindFsmInt("MP Cost").Value);
                    GameObject spawnedBomb = GameObject.Instantiate(BombManager.Bomb);
                    spawnedBomb.transform.localPosition = HeroController.instance.transform.localPosition;
                    spawnedBomb.transform.localScale = new(2f,2f,1f);
                    spawnedBomb.name = "Bomb";
                    spawnedBomb.GetComponent<Bomb>().Type = BombManager.BombQueue[0];
                    //BombManager.TakeBombs();
                    spawnedBomb.SetActive(true);
                })
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
                    GameObject spawnedBomb = GameObject.Instantiate(BombManager.Bomb);
                    spawnedBomb.transform.localPosition = HeroController.instance.transform.localPosition;
                    spawnedBomb.transform.localScale = new(2f,2f,1f);
                    spawnedBomb.name = "Power bomb";
                    spawnedBomb.SetActive(true);
                })
            }
        };
        powerBomb.AddTransition("FINISHED", "Spell End");

        // Add the transitions from the control. The actual call is done in the action hook.
        fsm.GetState("Spell Choice").AddTransition("BOMB", normalBomb);
        fsm.GetState("Spell Choice").AddTransition("POWERBOMB", powerBomb);
        fsm.GetState("QC").AddTransition("BOMB", normalBomb);
        fsm.GetState("QC").AddTransition("POWERBOMB", powerBomb);
    } 

    #endregion
}
