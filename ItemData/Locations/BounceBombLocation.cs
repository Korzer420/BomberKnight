using BomberKnight.UnityComponents;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System;
using UnityEngine;

namespace BomberKnight.ItemData.Locations;

// Statue position: 52.412, 18.41
// Room Ruins1_06
internal class BounceBombLocation : AutoLocation
{
    internal static GameObject Sentry { get; set; }

    protected override void OnLoad()
    {
        throw new NotImplementedException();
    }

    protected override void OnUnload()
    {
        throw new NotImplementedException();
    }

    private void StartFight()
    {
        GameObject sentry = GameObject.Instantiate(Sentry);
        PlayMakerFSM fsm = sentry.LocateMyFSM("Ruins Sentry Fat");
        // The enenmy should be stuck in the following circle:
        // Run Stop -> Single Antic -> (Attack States) -> Attack CD -> Run Stop
        fsm.GetState("Init").AdjustTransition("FINISHED", "Run Stop");

        // Prevent the enemy from exiting the attack cycle.
        fsm.GetState("Out Of Range?").AdjustTransition("OUT OF RANGE", "Run Stop");
        fsm.GetState("Out Of Range?").AdjustTransition("FINISHED", "Run Stop");

        fsm.GetState("Run Stop").AdjustTransition("FINISHED", "Single Antic");
        fsm.GetState("Attack CD").AdjustTransition("FINISHED", "Run Stop");

        fsm.GetState("Run Stop").AddLastAction(new Wait() { time = new HutongGames.PlayMaker.FsmFloat() { Value = 0.1f } });
        sentry.AddComponent<ItemDropper>().Placement = Placement;
        sentry.SetActive(true);
    }
}
