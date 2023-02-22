using BomberKnight.Enums;
using BomberKnight.UnityComponents;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BomberKnight.ItemData.Locations;

internal class EdgeBombBagLocation : AutoLocation
{
    public static GameObject Shockwave { get; set; }

    protected override void OnLoad()
    {
        Events.AddFsmEdit(new("Giant Hopper", "Hopper"), CreateHopperFight);
        On.HealthManager.Die += HealthManager_Die;
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (self.name == "Sated Hopper")
            self.GetComponent<ItemDropper>().PrepareDrop();
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(new("Giant Hopper", "Hopper"), CreateHopperFight);
    }

    private void CreateHopperFight(PlayMakerFSM fsm)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Deepnest_East_16")
            return;
        if (Placement.Items.Any(x => !x.IsObtained()))
        {
            HealthManager hopper = fsm.GetComponent<HealthManager>();
            hopper.hp = 360;
            hopper.gameObject.name = "Sated Hopper";
            hopper.gameObject.AddComponent<ItemDropper>().Firework = false;
            hopper.gameObject.GetComponent<ItemDropper>().Placement = Placement;
            if (Placement.Items.All(x => x.WasEverObtained()))
                ItemHelper.SpawnShiny(new(40f, 3.41f), Placement);
            else
            {
                fsm.GetState("Land Anim").AddLastAction(new Lambda(() =>
                {
                    LogHelper.Write<BomberKnight>("Spawn shockwave");
                    GameObject shockwaveLeft = GameObject.Instantiate(Shockwave);
                    shockwaveLeft.transform.localScale = new(1.4f, 1.4f);
                    shockwaveLeft.transform.position = new Vector3(fsm.transform.position.x, 4.2962f - 1.28f);
                    shockwaveLeft.SetActive(false);
                    PlayMakerFSM shockwaveFsm = shockwaveLeft.LocateMyFSM("shockwave");
                    shockwaveFsm.FsmVariables.FindFsmFloat("Speed").Value = 25f + ((360 - hopper.hp) / 30);
                    shockwaveFsm.FsmVariables.FindFsmBool("Facing Right").Value = false;

                    GameObject shockWaveRight = GameObject.Instantiate(Shockwave);
                    shockWaveRight.transform.localScale = new(1.4f, 1.4f);
                    shockWaveRight.transform.position = new Vector3(fsm.transform.position.x, 4.2962f - 1.28f);
                    shockWaveRight.SetActive(false);
                    shockwaveFsm = shockWaveRight.LocateMyFSM("shockwave");
                    shockwaveFsm.FsmVariables.FindFsmFloat("Speed").Value = 25f + ((360 - hopper.hp) / 30);
                    shockwaveFsm.FsmVariables.FindFsmBool("Facing Right").Value = true;

                    shockwaveLeft.SetActive(true);
                    shockWaveRight.SetActive(true);
                }));
                int jumpCounter = 0;
                fsm.AddState(new HutongGames.PlayMaker.FsmState(fsm.Fsm)
                {
                    Name = "Check Throw",
                    Actions = new HutongGames.PlayMaker.FsmStateAction[]
                    {
                        new Lambda(() =>
                        {
                            if (jumpCounter > 5 && UnityEngine.Random.Range(0, 5) == 0)
                            { 
                                jumpCounter = 0;
                                GameManager.instance.StartCoroutine(ThrowBombs(fsm.gameObject));
                            }
                            else
                            {
                                jumpCounter++;
                                fsm.SendEvent("FINISHED");
                            }
                        }),
                        new Wait()
                        {
                            time = 4.2f
                        }
                    }
                });
                fsm.GetState("Land Anim").AdjustTransition("FINISHED", "Check Throw");
                fsm.GetState("Check Throw").AddTransition("FINISHED", "Check Alert");
            }
        }
    }

    private IEnumerator ThrowBombs(GameObject hopper)
    {
        float passedTime = 0f;
        float bombCooldown = 0.25f;
        while(passedTime < 4f && hopper != null)
        {
            passedTime += Time.deltaTime;
            bombCooldown -= Time.deltaTime;
            if (bombCooldown <= 0f)
            {
                GameObject thrownBomb = GameObject.Instantiate(BombManager.Bomb);
                thrownBomb.transform.position = hopper.transform.position;
                thrownBomb.transform.localScale = new(2f, 2f, 1f);
                thrownBomb.name = "Bomb";
                thrownBomb.GetComponent<Bomb>().Type = BombType.GrassBomb;
                thrownBomb.GetComponent<Bomb>().EnemyBomb = true;
                thrownBomb.SetActive(true);
                FlingUtils.FlingObject(new FlingUtils.SelfConfig()
                {
                    AngleMax = 180f,
                    AngleMin = 0f,
                    SpeedMax = 50f,
                    SpeedMin = 30f,
                    Object = thrownBomb
                }, hopper.transform, new(0f, 5f));
                bombCooldown = 0.35f;
            }
            yield return null;
        }
    }
    //Deepnest_East_16 -> Giant Hopper
    // Ruins1_24
}
