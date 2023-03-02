using BomberKnight.Enums;
using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BomberKnight.ItemData.Locations;

// Fungus1_32
internal class GreenpathBombBagLocation : AutoLocation
{
    #region Control

    protected override void OnLoad()
    {
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
        On.HealthManager.Die += HealthManager_Die;
        Events.AddFsmEdit(new FsmID("Moss Knight C", "Moss Knight Control"), ModifyMossKnight);
    }

    protected override void OnUnload()
    {
        On.HealthManager.Die -= HealthManager_Die;
        Events.RemoveFsmEdit(new FsmID("Moss Knight C", "Moss Knight Control"), ModifyMossKnight);
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
    } 

    #endregion

    #region Event Handler

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("grass ball control", null, "Break"))
        {
            GameObject gameObject = GameObject.Instantiate(Bomb.Explosion);
            Component.Destroy(gameObject.LocateMyFSM("damages_enemy"));
            gameObject.transform.position = self.Fsm.GameObject.transform.position;
            ParticleSystem.MainModule settings = gameObject.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(Color.green);
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
            gameObject.GetComponentInChildren<SimpleSpriteFade>().fadeInColor = Color.green;

            typeof(SimpleSpriteFade).GetField("normalColor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(gameObject.GetComponentInChildren<SimpleSpriteFade>(), Color.green);
            gameObject.SetActive(true);
        }
        orig(self);
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (self.gameObject.name.StartsWith("Moss Knight") && self.GetComponent<ItemDropper>() is ItemDropper itemDropper)
            itemDropper.PrepareDrop();
    }

    private void ModifyMossKnight(PlayMakerFSM fsm)
    {
        if (fsm.gameObject.scene.name == "Fungus1_32" && Placement.Items.Any(x => !x.IsObtained()))
        {
            fsm.AddState(new HutongGames.PlayMaker.FsmState(fsm.Fsm)
            {
                Name = "Start Enhancement",
                Actions = new HutongGames.PlayMaker.FsmStateAction[]
                {
                    new Lambda(() => GameManager.instance.StartCoroutine(SpawnBombs(fsm.gameObject)))
                }
            });
            fsm.GetState("Wake").AdjustTransition("FINISHED", "Start Enhancement");
            fsm.GetState("Start Enhancement").AddTransition("FINISHED", "Reset");
            fsm.GetComponent<HealthManager>().hp *= 4;
            ItemDropper itemDropper = fsm.gameObject.AddComponent<ItemDropper>();
            itemDropper.Firework = false;
            itemDropper.Placement = Placement;
        }
    }

    #endregion

    private IEnumerator SpawnBombs(GameObject mossKnight)
    {
        HealthManager healthManager = mossKnight.GetComponent<HealthManager>();
        yield return new WaitForSeconds(3f);
        while (mossKnight != null && healthManager.hp > 0)
        {
            GameObject leftBomb = new("Bomb");
            leftBomb.transform.localPosition = mossKnight.transform.localPosition - new Vector3(2.5f, 0f);
            leftBomb.transform.localScale = new(2f, 2f, 1f);
            EnemyBomb projectile = leftBomb.AddComponent<EnemyBomb>();
            projectile.CollisionBehaviour = new()
            {
                ExplodeOnAttack = true,
                ExplodeOnHero = true,
                ExplodeOnTerrain = true,
            };
            projectile.WithGravity = true;
            projectile.Tick = true;
            projectile.ExplosionColor = Color.green;
            leftBomb.SetActive(true);

            GameObject rightBomb = new("Bomb");
            rightBomb.transform.localPosition = mossKnight.transform.localPosition + new Vector3(2.5f, 0f);
            rightBomb.transform.localScale = new(2f, 2f, 1f);
            projectile = rightBomb.AddComponent<EnemyBomb>();
            projectile.CollisionBehaviour = new()
            {
                ExplodeOnAttack = true,
                ExplodeOnHero = true,
                ExplodeOnTerrain = true,
            };
            projectile.WithGravity = true;
            projectile.Tick = true;
            projectile.ExplosionColor = Color.green;
            rightBomb.SetActive(true);
            yield return null;

            leftBomb.GetComponent<Rigidbody2D>().AddForce(new(Random.Range(-20f, 5f), Random.Range(1f, 20f)), ForceMode2D.Impulse);
            rightBomb.GetComponent<Rigidbody2D>().AddForce(new(Random.Range(20f, 5f), Random.Range(1f, 20f)), ForceMode2D.Impulse);
            yield return new WaitForSeconds(5f);
        }
    }
}
