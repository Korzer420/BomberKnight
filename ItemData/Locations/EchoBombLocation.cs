using BomberKnight.UnityComponents;
using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BomberKnight.ItemData.Locations;

internal class EchoBombLocation : AutoLocation
{
    #region Members

    private static readonly Vector3[] _benchmarks = new Vector3[]
    {
        new(21.5f, 63.5f),
        new(17.6f, 50.35f),
        new(9.33f, 26f),
        new(3.267f, 24.41f)
    };

    private bool _ghostEntered = false;

    #endregion
    // 3.267x, 24.41y Transition
    // First bomb -> 22.61x, 75.44y
    // Second bomb -> 21.5x, 63.5y
    // Third bomb ->17.6x, 50.35y
    // Forth bomb -> 9.33x, 26y

    // Shiny at: 50.67x, 3.41y (Resting Grounds_17)
    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("RestingGrounds_17", CheckForEcho);
        On.TransitionPoint.GetGatePosition += TransitionPoint_GetGatePosition;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if (arg1.name == "RestingGrounds_05")
            GameManager.instance.StartCoroutine(ShowEcho());
        if (arg1.name != "RestingGrounds_17" || arg0.name != "RestingGrounds_05")
            _ghostEntered = false;
    }

    private GlobalEnums.GatePosition TransitionPoint_GetGatePosition(On.TransitionPoint.orig_GetGatePosition orig, TransitionPoint self)
    {
        if (self.gameObject.name == "left4" && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "RestingGrounds_05")
            _ghostEntered = true;
        
        return orig(self);
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("RestingGrounds_17", CheckForEcho);
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void CheckForEcho(Scene scene)
    {
        if (!_ghostEntered || Placement.Items.All(x => x.IsObtained()))
            return;
        GameObject echoBombSpawn = new("Echo Bomb location");
        echoBombSpawn.transform.localPosition = new(50.67f, 5.41f);
        echoBombSpawn.SetActive(true);
        echoBombSpawn.AddComponent<ItemDropper>().Placement = Placement;
        echoBombSpawn.GetComponent<ItemDropper>().DropPosition = new(50.67f, 5.41f);
        echoBombSpawn.GetComponent<ItemDropper>().FireworkColor = new(1f,0f,1f);
        echoBombSpawn.GetComponent<ItemDropper>().PrepareDrop();
    }

    private IEnumerator ShowEcho()
    {
        yield return new WaitForSeconds(.2f);
        GameObject gate = null;
        GameObject bombSprite = null;
        SpriteRenderer spriteRenderer = null;
        try
        {
            gate = GameObject.Instantiate(GameObject.Find("left3"));
            gate.transform.localPosition = new(3.267f, 24.41f);
            gate.name = "left4";
            gate.GetComponent<TransitionPoint>().targetScene = "RestingGrounds_17";
            gate.SetActive(false);
            GameObject bombWall = new("Bomb Wall");
            bombWall.AddComponent<BoxCollider2D>().size = gate.GetComponent<BoxCollider2D>().size;
            bombWall.AddComponent<BombWall>().NewGate = gate;
            bombWall.transform.localPosition = gate.transform.localPosition;
            bombWall.transform.localScale = gate.transform.localScale;
            bombWall.layer = 7;
            bombWall.SetActive(true);

            bombSprite = new("Ghost Bomb");
            bombSprite.transform.localPosition = new(22.61f, 75.44f);
            bombSprite.transform.localScale = new(2f, 2f, 2f);
            spriteRenderer = bombSprite.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = SpriteHelper.CreateSprite<BomberKnight>("BombSprite");
            spriteRenderer.color = new(1f, 0f, 1f, 1f);
            bombSprite.SetActive(false);
        }
        catch (Exception exception)
        {
            LogHelper.Write<BomberKnight>("Couldn't show echo bomb: " + exception.ToString(), KorzUtils.Enums.LogType.Error);
        }

        // Used for calculate transparency.
        float maxDistance = Vector3.Distance(gate.transform.localPosition, bombSprite.transform.localPosition);
        int counter = 0;
        if (Placement.Items.All(x => x.IsObtained()))
            yield break;
        bombSprite.SetActive(true);
        while (bombSprite != null)
        {
            Vector3 targetPosition = _benchmarks[counter];
            float currentDistance = Vector3.Distance(targetPosition, bombSprite.transform.localPosition);
            while (currentDistance > 0.1f && bombSprite != null)
            {
                bombSprite.transform.localPosition = Vector3.MoveTowards(bombSprite.transform.localPosition, targetPosition, 0.005f);
                currentDistance = Vector3.Distance(targetPosition, bombSprite.transform.localPosition);
                spriteRenderer.color = new(1f, 0f, 1f, Vector3.Distance(gate.transform.localPosition, bombSprite.transform.localPosition) / maxDistance);
                yield return null;
            }
            if (bombSprite == null)
                break;
            counter++;
            if (counter > 3)
            {
                counter = 0;
                bombSprite.transform.localPosition = new(22.61f, 75.44f);
            }
        }

    }
}
