using BomberKnight.Enums;
using BomberKnight.UnityComponents;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BomberKnight.ItemData.Locations;

internal class EdgeBombBagLocation : AutoLocation
{
    #region Properties

    public static GameObject Shockwave { get; set; }

    #endregion

    #region Event handler

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (self.name == "Sated Hopper")
            self.GetComponent<ItemDropper>().PrepareDrop();
    }

    #endregion

    #region Control

    protected override void OnLoad()
    {
        Events.AddFsmEdit(new("Giant Hopper", "Hopper"), CreateHopperFight);
        On.HealthManager.Die += HealthManager_Die;
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(new("Giant Hopper", "Hopper"), CreateHopperFight);
        On.HealthManager.Die -= HealthManager_Die;
    }

    #endregion

    #region Methods

    private void CreateHopperFight(PlayMakerFSM fsm)
    {
        if (fsm.gameObject.scene.name != "Deepnest_East_16")
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
                    shockwaveLeft.transform.localScale = new(1.25f, 1.25f);
                    shockwaveLeft.transform.position = new Vector3(fsm.transform.position.x, 4.2962f - 1.28f);
                    shockwaveLeft.SetActive(false);
                    PlayMakerFSM shockwaveFsm = shockwaveLeft.LocateMyFSM("shockwave");
                    shockwaveFsm.FsmVariables.FindFsmFloat("Speed").Value = 25f + ((360 - hopper.hp) / 30);
                    shockwaveFsm.FsmVariables.FindFsmBool("Facing Right").Value = false;

                    GameObject shockWaveRight = GameObject.Instantiate(Shockwave);
                    shockWaveRight.transform.localScale = new(1.25f, 1.25f);
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
        while (passedTime < 4f && hopper != null)
        {
            passedTime += Time.deltaTime;
            bombCooldown -= Time.deltaTime;
            if (bombCooldown <= 0f)
            {
                GameObject enemyBomb = new("Spiderbomb");
                enemyBomb.SetActive(false);
                enemyBomb.transform.localPosition = hopper.transform.position + new Vector3(0f, 2f);
                enemyBomb.transform.localScale = new(2f, 2f);
                EnemyBomb projectile = enemyBomb.AddComponent<EnemyBomb>();
                projectile.CollisionBehaviour = new()
                {
                    ExplodeOnAttack = true,
                    ExplodeOnHero = true,
                    ExplodeOnTerrain = true,
                };
                projectile.WithGravity = true;
                projectile.Tick = true;
                projectile.ExplosionColor = new(0.8f, 0f, 0.2f);
                enemyBomb.SetActive(true);
                yield return null;
                enemyBomb.GetComponent<Rigidbody2D>().AddForce(new(Random.Range(-20f, 20f), Random.Range(1f, 20f)), ForceMode2D.Impulse);
                bombCooldown = 0.8f;
            }
            yield return null;
        }
    } 

    #endregion
}
