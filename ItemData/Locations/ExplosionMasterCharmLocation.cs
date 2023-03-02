using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using Modding;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

// Mines_20 -> 68.1f, 148.8f
// Fungus1_24 -> 88.02f, 22.1f
internal class ExplosionMasterCharmLocation : AutoLocation
{
    #region Members

    private GameObject _crystalBug;

    #endregion

    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Fungus1_24", QuestBombSpawn);
        Events.AddSceneChangeEdit("Mines_20", SpawnCrystal);
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
    }

    private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "GRASSHOPPER_TALK")
            return "What do you mean? He was sealed in a crystal anyway. Yeah, but you could've used this to help him. I feel like with all those crystals around them, the quake would've killed me as well.";
        return orig;
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Fungus1_24", QuestBombSpawn);
        Events.RemoveSceneChangeEdit("Mines_20", SpawnCrystal);
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
    }

    private void QuestBombSpawn(Scene scene)
    {
        if (BombManager.BombBagLevel == 0 || BombManager.BombQueue.Any(x => x == Enums.BombType.MiningBomb) 
            || Placement.AllObtained() || Placement.Items.All(x => x.WasEverObtained()))
            return;
        GameObject dropObject = new("Bomb drop");
        dropObject.AddComponent<Rigidbody2D>().mass = 10f;
        dropObject.AddComponent<CircleCollider2D>();
        dropObject.AddComponent<BombPickup>().Bombs = new() { Enums.BombType.MiningBomb };
        dropObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
        dropObject.SetActive(true);
        dropObject.transform.localScale = new(2.5f, 2.5f);
        dropObject.transform.position = new(88.02f, 22.1f);
    }

    private void SpawnCrystal(Scene scene)
    { 
        if (!Placement.AllObtained())
        {
            if (Placement.Items.All(x => x.WasEverObtained()))
                ItemHelper.SpawnShiny(new(68.1f, 148.8f), Placement);
            else
            {
                _crystalBug = new("Bomb Master");
                _crystalBug.layer = 11;
                _crystalBug.AddComponent<TinkEffect>();
                _crystalBug.transform.position = new(68.1f, 151.5901f, 0f);
                _crystalBug.transform.localScale = new(2f, 2f);
                _crystalBug.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.Frozen_Explosion_Master");
                _crystalBug.AddComponent<BombWall>().Bombed += ExplosionMasterCharmLocation_Bombed;
            }
        }
    }

    private bool? ExplosionMasterCharmLocation_Bombed(string explosionName)
    {
        if (explosionName.Contains("Mining"))
        {
            ItemHelper.FlingShiny(_crystalBug, Placement);
            return true;
        }
        return false;
    }
}
