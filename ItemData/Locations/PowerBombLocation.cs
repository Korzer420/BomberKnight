using BomberKnight.BombElements;
using BomberKnight.Data;
using BomberKnight.Enums;
using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

internal class PowerBombLocation : AutoLocation
{
    private List<BombType> _placedBombs = new();
    private GameObject[] _indicators = new GameObject[5];

    protected override void OnLoad()
    {
        Bomb.BombExploded += Bomb_BombExploded;
        Events.AddSceneChangeEdit("Abyss_08", Spawn);
    }

    private void Bomb_BombExploded(BombEventArgs bombEventArgs)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Abyss_08" && !_placedBombs.Contains(bombEventArgs.Type)
            && bombEventArgs.Position.x >= 76 && bombEventArgs.Position.x <= 81
            && bombEventArgs.Position.y >= 24.5f && bombEventArgs.Position.y <= 25f)
        {
            _placedBombs.Add(bombEventArgs.Type);
            _indicators[(int)bombEventArgs.Type].GetComponent<SpriteRenderer>().color = BombManager.GetBombColor(bombEventArgs.Type);
            if (_placedBombs.Count == 5)
                GameManager.instance.StartCoroutine(CreatePowerBomb());
        }
    }

    protected override void OnUnload()
    {
        Bomb.BombExploded -= Bomb_BombExploded;
        Events.RemoveSceneChangeEdit("Abyss_08", Spawn);
    }

    private void Spawn(Scene scene)
    {
        _placedBombs.Clear();
        if (Placement.Items.Any(x => !x.IsObtained()))
        {
            if (Placement.Items.All(x => x.WasEverObtained()))
                ItemHelper.SpawnShiny(new(78.47f, 25.5f), Placement);
            else
            {
                Vector3[] positions = new Vector3[]
                {
                    new(74.47f, 27.23f),
                    new(76.47f, 29.23f),
                    new(78.47f, 31.23f),
                    new(80.47f, 29.23f),
                    new(82.47f, 27.23f)
                };
                for (int i = 0; i < 5; i++)
                {
                    GameObject flyingBomb = new("Indicator " + i);
                    flyingBomb.transform.position = positions[i];
                    Color bombColor = BombManager.GetBombColor((BombType)i);
                    flyingBomb.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
                    flyingBomb.GetComponent<SpriteRenderer>().color = new(bombColor.r, bombColor.g, bombColor.b, 0.2f);
                    flyingBomb.transform.localScale = new(2f, 2f, 1f);
                    flyingBomb.SetActive(true);
                    _indicators[i] = flyingBomb;
                }
            }
        }
    }

    private IEnumerator CreatePowerBomb()
    {
        GameObject center = new("Center");
        center.transform.position = new(78.47f, 29.23f);
        center.transform.localScale = new(1f, 1f);
        center.SetActive(true);
        Vector3[] movementSteps = new Vector3[]
        {
            new Vector3(0.618f, 1.9f),
            new Vector3(-1.618f, 1.175f),
            new Vector3(-1.618f, -1.175f),
            new Vector3(0.618f, -1.9f),
            new Vector3(2, 0)
        };

        for (int i = 0; i < 5; i++)
        { 
            _indicators[i].transform.SetParent(center.transform);
            while (Vector3.Distance(_indicators[i].transform.localPosition, movementSteps[i]) > 0.1f)
            { 
                _indicators[i].transform.localPosition = Vector3.MoveTowards(_indicators[i].transform.localPosition, movementSteps[i], 0.02f);
                yield return null;
            }
        }

        float passedTime = 0f;
        while(passedTime < 10f)
        {
            passedTime += Time.deltaTime;
            center.transform.eulerAngles += new Vector3(0f, 0f, passedTime);
            if (passedTime <= 4.5f)
                for (int i = 0; i < 5; i++)
                    _indicators[i].transform.localPosition += new Vector3(movementSteps[i].x * Time.deltaTime, movementSteps[i].y * Time.deltaTime);
            else
                for (int i = 0; i < 5; i++)
                    _indicators[i].transform.localPosition -= new Vector3(movementSteps[i].x * Time.deltaTime, movementSteps[i].y * Time.deltaTime);
            yield return null;
        }
        Bomb.FakeExplosion(center.transform.position, Color.cyan, new(4f, 4f));
        ItemHelper.FlingShiny(center, Placement);
        GameObject.Destroy(center);
    }
}
