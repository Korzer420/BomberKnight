using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

internal class GoldFountainLocation : AutoLocation
{
    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Ruins2_04", CreateFountain);
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Ruins2_04", CreateFountain);
    }

    private void CreateFountain(Scene scene)
    {
        if (Placement.Items.Any(x => !x.IsObtained()))
            GameManager.instance.StartCoroutine(WaitForEntry());   
    }

    private IEnumerator WaitForEntry()
    {
        yield return new WaitUntil(() => HeroController.instance.acceptingInput);
        GameObject water1 = GameObject.Find("lake_water_v02").transform.Find("ruin_water_top_v02_002 (12)")?.gameObject;
        if (water1 == null)
            yield break;
        water1.GetComponent<SpriteRenderer>().color = Color.yellow;

        GameObject water2 = GameObject.Find("lake_water_v02").transform.Find("ruin_water_top_v02_002 (13)")?.gameObject;
        water2.GetComponent<SpriteRenderer>().color = Color.yellow;

        // 93.26x/3.26y - 100.7325x/3.26y

        GameObject fountain = new("Gold Fountain");
        fountain.transform.position = new Vector3(96.99625f, 0.97f);
        fountain.transform.localScale = new(1f, 1f, 1f);
        fountain.layer = 8;
        fountain.AddComponent<BoxCollider2D>().size = new(100.7325f - 93.26f, 2f);
        fountain.AddComponent<Fountain>();
        fountain.AddComponent<ItemDropper>().FireworkColor = Color.yellow;
        fountain.GetComponent<ItemDropper>().Placement = Placement;
        fountain.GetComponent<ItemDropper>().DropPosition = new(102f, 7.24f);
        fountain.SetActive(true);

        FlingGeoAction.SpawnGeo(10, 1, 0, ItemChanger.FlingType.Everywhere, new(93.26f, 7.26f));
        FlingGeoAction.SpawnGeo(10, 1, 0, ItemChanger.FlingType.Everywhere, new(100.7325f, 7.26f));
    }
}
