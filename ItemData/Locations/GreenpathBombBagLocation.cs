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

internal class GreenpathBombBagLocation : AutoLocation
{
    protected override void OnLoad()
    {
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter += SetVelocity2d_OnEnter;
        Events.AddFsmEdit(new FsmID("Moss Knight B", "Moss Knight Control"), ModifyMossKnight);
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(new FsmID("Moss Knight B", "Moss Knight Control"), ModifyMossKnight);
        On.HutongGames.PlayMaker.Actions.SetVelocity2d.OnEnter -= SetVelocity2d_OnEnter;
    }

    private void ModifyMossKnight(PlayMakerFSM fsm)
    {
        if (Placement.Items.Any(x => !x.IsObtained()))
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
            //fsm.gameObject.name = "Bomber " + fsm.gameObject.name;
        }
    }

    private IEnumerator SpawnBombs(GameObject mossKnight)
    {
        HealthManager healthManager = mossKnight.GetComponent<HealthManager>();
        yield return new WaitForSeconds(3f);
        while (mossKnight != null && healthManager.hp > 0)
        {
            GameObject leftBomb = GameObject.Instantiate(BombManager.Bomb);
            leftBomb.transform.localPosition = mossKnight.transform.localPosition - new Vector3(2.5f, 0f);
            leftBomb.transform.localScale = new(2f, 2f, 1f);
            leftBomb.name = "Bomb";
            leftBomb.GetComponent<Bomb>().Type = BombType.GrassBomb;
            leftBomb.GetComponent<Bomb>().EnemyBomb = true;
            leftBomb.SetActive(true);
            FlingUtils.FlingObject(new FlingUtils.SelfConfig()
            {
                AngleMax = 180f,
                AngleMin = 90f,
                SpeedMax = 10f,
                SpeedMin = 4f,
                Object = leftBomb
            }, mossKnight.transform, new(-3f, 3f));
            
            GameObject rightBomb = GameObject.Instantiate(BombManager.Bomb);
            rightBomb.transform.localPosition = mossKnight.transform.localPosition + new Vector3(2.5f, 0f);
            rightBomb.transform.localScale = new(2f, 2f, 1f);
            rightBomb.name = "Bomb";
            rightBomb.GetComponent<Bomb>().Type = BombType.GrassBomb;
            rightBomb.GetComponent<Bomb>().EnemyBomb = true;
            rightBomb.SetActive(true);
            FlingUtils.FlingObject(new FlingUtils.SelfConfig()
            {
                AngleMax = 90f,
                AngleMin = 0f,
                SpeedMax = 10f,
                SpeedMin = 4f,
                Object = rightBomb
            }, mossKnight.transform, new(-3f, 3f));
            yield return new WaitForSeconds(3f);
        }
    }

    private void SetVelocity2d_OnEnter(On.HutongGames.PlayMaker.Actions.SetVelocity2d.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetVelocity2d self)
    {
        if (self.IsCorrectContext("grass ball control", null, "Break"))
        {
            GameObject gameObject = GameObject.Instantiate(Bomb.Explosion);
            Component.Destroy(gameObject.LocateMyFSM("damages_enemy"));
            gameObject.transform.position = self.Fsm.GameObject.transform.position;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.green;
            gameObject.SetActive(true);
        }
        orig(self);
    }

}
