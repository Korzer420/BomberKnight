using BomberKnight.UnityComponents;
using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

// Fungus3_28
internal class PyromaniacCharmLocation : AutoLocation
{
    private GameObject _corpse;
    private List<GameObject> _explosionObjects = new();

    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Fungus3_28", SequenceWrapper);
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
    }

    private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "BomberKnight_Pyromaniac_Hint_40_1")
            orig = "More...";
        else if (key == "BomberKnight_Pyromaniac_Hint_30_1")
            orig = "The heat burning their skin off... what lucky bugs they are.";
        else if (key == "BomberKnight_Pyromaniac_Hint_20_1")
            orig = "The sound of destruction and fear are so precious.";
        else if (key == "BomberKnight_Pyromaniac_Hint_10_1")
            orig = "Only a few more... let all bugs shiver in their cold shell.";
        else if (key == "BomberKnight_Pyromaniac_Final_Hint_1")
            orig = "At last, let my corpse combust in the glorious light.";
        return orig;
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Fungus3_28", SequenceWrapper);
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
    }

    private void SequenceWrapper(Scene scene) => GameManager.instance.StartCoroutine(CreateSequence());

    private IEnumerator CreateSequence()
    {
        yield return null;
        _explosionObjects.Clear();
        if (Placement.Items.Any(x => !x.IsObtained()))
        {
            if (Placement.Items.All(x => x.WasEverObtained()))
                ItemHelper.SpawnShiny(new(71.67f, 31.42f), Placement);
            else
            {
                GameObject corpse = new("Pyromaniac");
                corpse.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("PyroCorpse");
                corpse.SetActive(true);
                corpse.transform.position = new(71.97f, 30.91f, 0f);
                corpse.transform.localScale = new(-1.5f, 1.5f);
                corpse.layer = 13;
                _corpse = corpse;
                foreach (JellyEgg egg in GameObject.FindObjectsOfType<JellyEgg>())
                {
                    if (egg.bomb && egg.gameObject.scene.name == "Fungus3_28")
                    {
                        egg.gameObject.AddComponent<JellyCounter>().Location = this;
                        _explosionObjects.Add(egg.gameObject);
                    }
                }
                foreach (HealthManager enemy in GameObject.FindObjectsOfType<HealthManager>())
                {
                    if (enemy.name.StartsWith("Jellyfish") && enemy.gameObject.scene.name == "Fungus3_28")
                    {
                        enemy.gameObject.AddComponent<JellyCounter>().Location = this;
                        _explosionObjects.Add(enemy.gameObject);
                    }
                }
            }
        }
    }

    internal void UpdateProgress(GameObject toRemove)
    {
        _explosionObjects.Remove(toRemove);

        if (!_explosionObjects.Any())
        {
            PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
            playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = 1;
            playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = "BomberKnight_Pyromaniac_Final_Hint";
            playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
            _corpse.AddComponent<BombWall>().Bombed += SpawnReward;
        }
        else if (_explosionObjects.Count % 10 == 0)
        {
            PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
            playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = 1;
            playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = "BomberKnight_Pyromaniac_Hint_"+_explosionObjects.Count;
            playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
        }
    }

    private bool? SpawnReward(string explosionName)
    {
        ItemHelper.FlingShiny(_corpse, Placement);
        GameObject.Destroy(_corpse);
        return true;
    }
}
