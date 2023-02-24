using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static tk2dSpriteCollectionDefinition;

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
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Fungus1_24", QuestBombSpawn);
        Events.RemoveSceneChangeEdit("Mines_20", SpawnCrystal);
    }

    private void QuestBombSpawn(Scene scene)
    {
        if (BombManager.BombQueue.Any(x => x == Enums.BombType.MiningBomb) 
            || Placement.AllObtained() || Placement.Items.All(x => x.WasEverObtained()))
            return;
        GameObject dropObject = new("Bomb drop");
        dropObject.AddComponent<Rigidbody2D>().mass = 10f;
        dropObject.AddComponent<CircleCollider2D>();
        dropObject.AddComponent<BombPickup>().Bombs = new() { Enums.BombType.MiningBomb };
        dropObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("BombSprite");
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
                _crystalBug.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Frozen_Explosion_Master");
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
