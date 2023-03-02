using BomberKnight.UnityComponents;
using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Locations;
using KorzUtils.Helper;
using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

// GG_Waterways Chest (3)
// position: 35.33f, 16.38f
// scale: 1.2f, 1.2f
internal class ShellSalvagerLocation : AutoLocation
{
    #region Members

    private readonly Dictionary<string, Vector3> _hintLocations = new()
    {
        {"Crossroads_10", new(36.52f, 3.41f) },
        {"Fungus3_48", new(40.88f, 94.41f) },
        {"Waterways_13", new(7.9f, 18.41f) },
        {"Waterways_15", new(14.09f, 4.41f) },
        {"Room_Mansion", new(22f, 6.41f) }
    };

    private Dictionary<string, string> _hintText = new()
    {
        {"Dryya_Hint_1", "The white chest should be hit {0}." },
        {"Isma_Hint_1", "My precious green chest should be the {0}." },
        {"Ogrim_Hint_1", "For our treasure my brown chest should be struck {0}." },
        {"Ze'mer_Hint_1", "To be the {0} one is the key of my blue chest." },
        {"Hegemol_Hint_1", "Remember, red has to be {0}." }
    };

    private readonly string[] _orderTerms = new string[]
    {
        "first",
        "second",
        "third",
        "fourth",
        "last"
    };

    private GameObject _chest;

    private List<string> _hitChests = new();

    #endregion

    #region Properties

    public static List<string> ChestOrder { get; set; } = new();

    #endregion

    // Crossroads_10 -> 36.52f, 3.41f
    // Fungus3_48 -> 40.88f, 94.41f
    // Waterways_13 -> 97.9f, 18.41f
    // Waterways_15 -> 14.09f, 4.41f
    // Room_Mansion -> 22f, 6.41f
    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Crossroads_10", SpawnHintbox);
        Events.AddSceneChangeEdit("Fungus3_48", SpawnHintbox);
        Events.AddSceneChangeEdit("Waterways_13", SpawnHintbox);
        Events.AddSceneChangeEdit("Waterways_15", SpawnHintbox);
        Events.AddSceneChangeEdit("Room_Mansion", SpawnHintbox);
        Events.AddSceneChangeEdit("GG_Waterways", WaitForSpawn);
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
    }

    internal static void RollOrder(int seed)
    {
        System.Random random = seed != -1 
            ? new System.Random(seed) 
            : new();
        ChestOrder.Clear();
        List<string> knights = new() { "Dryya", "Isma", "Ogrim", "Ze'mer", "Hegemol" };
        for (int i = 0; i < 5; i++)
        {
            ChestOrder.Add(knights[random.Next(0, knights.Count)]);
            knights.Remove(ChestOrder.Last());
            LogHelper.Write<BomberKnight>("Next knight is: " + ChestOrder[i]);
        }
    }

    private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (_hintText.ContainsKey(key))
        {
            string knightName = new(key.TakeWhile(x => x != '_').ToArray());
            string hint = string.Format(_hintText[key], _orderTerms[ChestOrder.IndexOf(knightName)]);
            if (BombManager.ColorlessHelp)
                hint = hint.Replace("white", "middle one")
                    .Replace("blue", "leftmost")
                    .Replace("red", "rightmost")
                    .Replace("green", "second from right")
                    .Replace("brown", "second from left");
            return hint;
        }
        return orig;
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Crossroads_10", SpawnHintbox);
        Events.RemoveSceneChangeEdit("Fungus3_48", SpawnHintbox);
        Events.RemoveSceneChangeEdit("Waterways_13", SpawnHintbox);
        Events.RemoveSceneChangeEdit("Waterways_15", SpawnHintbox);
        Events.RemoveSceneChangeEdit("Room_Mansion", SpawnHintbox);
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
    }

    private void SpawnHintbox(Scene scene)
    {
        if (Placement.Items.All(x => x.WasEverObtained()))
            return;
        GameObject hitbox = new("Hint");
        hitbox.layer = 17;
        hitbox.AddComponent<BombWall>().Bombed += (x) =>
        {
            if (x.StartsWith("Echo"))
            {
                PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
                playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = 1;
                playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = scene.name switch
                {
                    "Crossroads_10" => "Hegemol_Hint",
                    "Fungus3_48" => "Dryya_Hint",
                    "Waterways_13" => "Isma_Hint",
                    "Waterways_15" => "Ogrim_Hint",
                    _ => "Ze'mer_Hint"
                };
                playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
            }
            return false;
        };
        hitbox.AddComponent<NonBouncer>();
        hitbox.transform.localScale = new(2f, 2f);
        hitbox.transform.position = _hintLocations[scene.name];
        hitbox.SetActive(true);
    }

    private void WaitForSpawn(Scene scene) => GameManager.instance.StartCoroutine(SpawnChest());

    private IEnumerator SpawnChest()
    {
        _hitChests.Clear();
        KnightChest.Location = this;
        yield return null;
        if (!Placement.AllObtained())
        {
            if (Placement.Items.All(x => x.WasEverObtained()))
                ItemHelper.SpawnShiny(new(35.33f, 16.38f), Placement);
            else
            {
                GameObject blockEffect = null;
                foreach (NonThunker nonThunker in GameObject.FindObjectsOfType<NonThunker>(true).ToList())
                    if (nonThunker.gameObject.name.StartsWith("Chest") && nonThunker.gameObject.name != "Chest(Clone)")
                    {   
                        nonThunker.gameObject.AddComponent<KnightChest>();
                        GameObject child = nonThunker.transform.Find("Opened").gameObject;
                        child.AddComponent<KnightChest>();
                        nonThunker.GetComponent<tk2dSprite>().color = nonThunker.gameObject.name switch
                        {
                            "Chest" => Color.green,
                            "Chest (1)" => Color.white,
                            "Chest (2)" => new(0.6f, 0.2f, 0.01f),
                            "Chest (3)" => Color.blue,
                            _ => Color.red
                        };

                        // Color the opened chests as well.
                        foreach (tk2dSprite sprite in child.transform.GetComponentsInChildren<tk2dSprite>())
                            sprite.color = nonThunker.GetComponent<tk2dSprite>().color;
                        if (nonThunker.gameObject.name == "Chest")
                            blockEffect = nonThunker.transform.GetComponentInChildren<TinkEffect>(true).blockEffect;
                    }

                _chest = new("Big Chest");
                _chest.transform.position = new(34.23f, 15.4091f, 0f);
                _chest.transform.localScale = new(2f, 2f);
                _chest.layer = 8;
                _chest.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Chest");
                _chest.AddComponent<BoxCollider2D>();
                _chest.AddComponent<TinkEffect>().blockEffect = blockEffect;
                _chest.SetActive(true);
            }
        }
    }

    internal void HitChest(string name)
    {
        // Chest (3) left most (Ze'mer)
        // Chest (2) second left most (Ogrim)
        // Chest (1) middle one (Dryya)
        // Chest second from right (Isma)
        // Chest (4) right most (Hegemol)
        string knightName = name switch
        {
            "Chest" => "Isma",
            "Chest (1)" => "Dryya",
            "Chest (2)" => "Ogrim",
            "Chest (3)" => "Ze'mer",
            _ => "Hegemol"
        };

        // To prevent multiple collider throwing the same name.
        if (_hitChests.Any() && _hitChests.Last() == name)
            return;
        _hitChests.Add(knightName);

        if (_hitChests.Count == 5)
        {
            LogHelper.Write<BomberKnight>("Compare solution");
            
            for (int i = 0; i < 5; i++)
            {
                if (ChestOrder[i] != _hitChests[i])
                {
                    _hitChests.RemoveAt(0);
                    return;
                }
            }
            _chest.GetComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Chest_Open");
            Component.Destroy(_chest.GetComponent<BoxCollider2D>());
            ItemHelper.FlingShiny(_chest, Placement);
        }
    }
}
