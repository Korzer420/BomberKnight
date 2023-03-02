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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

// Deepnest_10
internal class DeepnestBombBagLocation : AutoLocation
{
    #region Properties

    public static GameObject Spider { get; set; }

    #endregion

    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Deepnest_10", CreateEncounter);
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Deepnest_10", CreateEncounter);
    }

    private void CreateEncounter(Scene scene) => GameManager.instance.StartCoroutine(CreateArena());

    private IEnumerator CreateArena()
    {
        // Wait for frame for everything to be loaded.
        yield return null;
        // The big platform in the water.
        GameObject currentPlatform = GameObject.Instantiate(GameObject.Find("deepnest_platform_04"));
        currentPlatform.transform.position = new(30.25f, 4f);

        // The smaller platform directly above the big one.
        currentPlatform = GameObject.Instantiate(GameObject.Find("deepnest_platform_05"));
        currentPlatform.transform.position = new(30.25f, 17f);

        // Platforms on the left side.

        // The small platform next to the big one in the water.
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_07"));
        currentPlatform.transform.position = new(11.75f, 4f);

        // The small platform above the small in the water.
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_12"));
        currentPlatform.transform.position = new(14f, 9.5f);

        // The block platform above the small in the water.
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_10"));
        currentPlatform.transform.position = new(7.8f, 13.6f);

        // The medium platform above the small in the water.
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_11"));
        currentPlatform.transform.position = new(16.75f, 15.7f);

        // The second small platform above the small in the water.
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_12"));
        currentPlatform.transform.position = new(12.9f, 21.6f);

        // Platforms on the right side.

        // The small platform next to the big one in the water.
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_07"));
        currentPlatform.transform.position = new(48.75f, 4f);

        // The small platform above the small in the water (right).
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_12"));
        currentPlatform.transform.position = new(46.5f, 9.5f);

        // The block platform above the small in the water (right).
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_10"));
        currentPlatform.transform.position = new(52.7f, 13.6f);

        // The medium platform above the small in the water (right).
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_11"));
        currentPlatform.transform.position = new(43.75f, 15.7f);

        // The second small platform above the small in the water (right).
        currentPlatform = GameObject.Instantiate(GameObject.Find("plat_float_12"));
        currentPlatform.transform.position = new(47.6f, 21.6f);

        if (Placement.Items.Any(x => !x.IsObtained()))
        {
            if (Placement.Items.All(x => x.WasEverObtained()))
                ItemHelper.SpawnShiny(new(30.25f, 4.5f), Placement);
            else
            {
                GameObject spiderBoss = GameObject.Instantiate(Spider);
                spiderBoss.name = "Spider boss";
                spiderBoss.SetActive(false);
                spiderBoss.transform.position = new(30.25f, 7f);
                PlayMakerFSM fsm = spiderBoss.LocateMyFSM("Control");
                fsm.GetState("Chase - In Sight").GetFirstActionOfType<ChaseObjectV2>().speedMax = 3f;
                fsm.GetState("Chase - Out of Sight").GetFirstActionOfType<ChaseObjectV2>().speedMax = 3f;
                fsm.transform.position += new Vector3(-4f, 4f);
                fsm.GetState("Idle").AddFirstAction(new Lambda(() =>
                {
                    if (fsm.GetComponent<DeepnestSpiderControl>() is null)
                        fsm.gameObject.AddComponent<DeepnestSpiderControl>();
                }));
                ItemDropper itemDropper = fsm.gameObject.AddComponent<ItemDropper>();
                itemDropper.Placement = Placement;
                itemDropper.Firework = false;
                itemDropper.DropPosition = new(31f, 6f);
                fsm.GetComponent<HealthManager>().hp = 400;
                spiderBoss.SetActive(true);
                fsm.GetComponent<tk2dSprite>().color = new(1f, 0f, 1f);
            }
        }
    }

    /*
     Platform locations:
    Big platform (deepnest_platform_04): 30.25f, 4f
    Smaller platform above (deepnest_platform_05): 30.25f, 17f
    
    Medium platform in the water (plat_float_07): 48.75f, 4f
    Small platform above medium platform (plat_float_12): 46.5f, 9.5f
    Small block above medium platform (plat_float_10): 52.7f, 13.6f
    Medium platform above medium platform (plat_float_11): 43.75f, 15.7f
    Small platform highest (plat_float_12): 47.6f, 21.6f
     */
}
