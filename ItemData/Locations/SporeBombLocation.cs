using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using Modding;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

internal class SporeBombLocation : AutoLocation
{
    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Fungus2_30", Spawn);
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
    }

    private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "FUNG_SHROOM_DREAM")
            orig = "Touched by the energy which grants us our life. Awakened by a blazing quake. And lastly, bathed in the essence created by our elder ones.";
        return orig;
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Fungus2_30", Spawn);
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
    }

    private void Spawn(Scene scene)
    {
        if (Placement.Items.All(x => !x.IsObtained()))
        {
            if (Placement.Items.All(x => x.WasEverObtained()))
                ItemHelper.SpawnShiny(new(63.52f, 22.41f), Placement);
            else
            {
                GameObject stone = new("Stone Bomb");
                stone.transform.position = new(63.52f, 20.91f);
                stone.transform.localScale = new(2f, 2f, 1f);
                stone.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
                stone.AddComponent<CircleCollider2D>();
                ItemDropper itemDropper = stone.AddComponent<ItemDropper>();
                itemDropper.Placement = Placement;
                itemDropper.Firework = true;
                itemDropper.FireworkColor = new(1f, 0.4f, 0f);
                stone.AddComponent<SporeBombSequence>();
                stone.layer = 11;
            }
        }
    }
}
