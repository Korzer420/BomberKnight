using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

internal class SporeBombLocation : AutoLocation
{
    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Fungus2_30", Spawn);
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Fungus2_30", Spawn);
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
                stone.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("BombSprite");
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
