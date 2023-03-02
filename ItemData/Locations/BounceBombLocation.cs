using BomberKnight.UnityComponents;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

// Statue position: 52.412, 18.41
// Room Ruins1_06
internal class BounceBombLocation : AutoLocation
{
    internal static GameObject Sentry { get; set; }

    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Ruins1_06", SpawnStatue);
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Ruins1_06", SpawnStatue);
    }

    private void SpawnStatue(Scene scene)
    {
        if (Placement.Items.All(x => x.WasEverObtained()) && Placement.Items.Any(x => !x.IsObtained()))
            ItemHelper.SpawnShiny(new(52.412f, 18.41f), Placement);
        else if (Placement.Items.Any(x => !x.IsObtained()))
        {
            GameObject statue = new("Statue");
            statue.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sentry_Stone");
            statue.AddComponent<BombWall>().Bombed += (x) =>
            {
                StartFight();
                statue.SetActive(false);
                return true;
            };
            statue.layer = 11;
            statue.SetActive(true);
            statue.transform.position = new(54.912f, 19.31f, 0.1f);
            statue.transform.localScale = new(2f, 2f, 1f);
        }
    }

    private void StartFight()
    {
        try
        {
            GameObject sentry = GameObject.Instantiate(Sentry);
            sentry.transform.position = new(57.24f, 21.3f, 0.004f);
            sentry.name = "Fire Sentry";
            sentry.AddComponent<BridgeGuardControl>();
            Component.Destroy(sentry.GetComponent<DamageHero>());
            sentry.transform.localScale = new(-1.2f, 1.2f, 1f);
            PlayMakerFSM fsm = sentry.LocateMyFSM("Ruins Sentry Fat");
            // The enemy should be stuck in the following circle:
            // Run Stop -> Single Antic -> (Attack States) -> Attack CD -> Run Stop
            fsm.GetState("Init").AdjustTransition("FINISHED", "Jump Antic");
            Component.Destroy(sentry.GetComponent<Walker>());
            fsm.GetState("Run Stop").AdjustTransition("FINISHED", "Single Antic");
            fsm.GetState("Attack CD").AdjustTransition("FINISHED", "Run Stop");

            HealthManager enemy = sentry.GetComponent<HealthManager>();
            enemy.hp = 5000;

            fsm.GetState("Single Swipe").AddLastAction(new Lambda(() =>
            {
                if (BridgeGuardControl.ReadyToJump)
                    return;
                GameObject bomb = new("Sentry projectile");
                bomb.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("BombSprite");
                bomb.AddComponent<CircleCollider2D>().isTrigger = true;
                bomb.GetComponent<CircleCollider2D>().radius = 0.28f;
                bomb.transform.localScale = new(2.5f, 2.5f, 1f);
                bomb.layer = 13;
                SentryAttack sentryAttack = bomb.AddComponent<SentryAttack>();
                sentryAttack.StartPosition = UnityEngine.Random.Range(1, 4);
                sentryAttack.FromLeft = sentry.transform.localPosition.x < 10f;
                if (enemy.hp == 1 || enemy.hp == 3)
                    sentryAttack.Move = MoveType.Random;
                else
                    sentryAttack.Move = (MoveType)enemy.hp;
                bomb.SetActive(true);
            }));
            fsm.GetState("Run Stop").AddLastAction(new Wait() { time = new HutongGames.PlayMaker.FsmFloat() { Value = 0.1f } });
            ItemDropper itemDropper = sentry.AddComponent<ItemDropper>();
            itemDropper.Placement = Placement;
            itemDropper.Firework = true;
            itemDropper.DropPosition = new(10f, 19.4f);

            bool intialize = true;
            fsm.GetState("Swipe").AddLastAction(new Lambda(() =>
            {
                sentry.transform.position = new(3.5595f, 18.4081f, 0.004f);
                if (intialize)
                {
                    enemy.hp = 5;
                    intialize = false;
                    GameManager.instance.StartCoroutine(SentryAttack.Respawn(HeroController.instance.transform.position, false, false));
                }
                BridgeGuardControl.ReadyToJump = false;

                if (enemy.hp % 2 == 0)
                {
                    sentry.transform.position = new Vector3(95.5f, 18.4081f, 0.004f);
                    BridgeGuardControl.IsLeft = false;
                    sentry.transform.localScale = new(1.2f, 1.2f);
                    HeroController.instance.SetHazardRespawn(new Vector3(3f, 18.41f), true);
                }
                else
                {
                    BridgeGuardControl.IsLeft = true;
                    sentry.transform.position = new Vector3(3.5595f, 18.4081f, 0.004f);
                    sentry.transform.localScale = new(-1.2f, 1.2f);
                    HeroController.instance.SetHazardRespawn(new Vector3(97f, 18.41f), false);
                    if (enemy.hp == 1)
                        fsm.GetState("Attack CD").GetFirstActionOfType<WaitRandom>().timeMax = 0.2f;
                }
            }));
            fsm.GetState("Launch").AddLastAction(new Lambda(() =>
            {
                if (intialize)
                    sentry.transform.position = new(57.24f, 21.3f, 0.004f);
            }));

            fsm.GetState("Attack CD").AddLastAction(new Lambda(() =>
            {
                if (BridgeGuardControl.ReadyToJump)
                {
                    fsm.FsmVariables.FindFsmFloat("Jump Distance").Value = sentry.transform.localPosition.x < 50 ? 150 : -150f;
                    fsm.SendEvent("EVADE");
                    Bomb.FakeExplosion(sentry.transform.position, Color.white, new(1f, 1f));
                }
            }));
            fsm.GetState("Attack CD").AddTransition("EVADE", "Jump Antic");
            sentry.SetActive(true);
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("Couldn't modify guard: " + exception.ToString());
        }
    }
}
