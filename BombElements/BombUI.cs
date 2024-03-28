using BomberKnight.Enums;
using BomberKnight.Resources;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using Modding;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BomberKnight.BombElements;

/// <summary>
/// Controls everything around the UI of the bombs. Includes the counter and inventory.
/// </summary>
public static class BombUI
{
    #region Members

    private static GameObject _tracker;

    private static GameObject _colorless;

    private static Dictionary<string, GameObject[]> _controlElements = new();

    private static Sprite _emptySprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Backboards/BB 3").GetComponent<SpriteRenderer>().sprite;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the tracker that displays the current bomb amount.
    /// </summary>
    public static GameObject Tracker
    {
        get
        {
            if (_tracker == null)
            {
                try
                {
                    GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
                    GameObject hudCanvas = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas").gameObject;
                    _tracker = Object.Instantiate(prefab, hudCanvas.transform, true);
                    _tracker.name = "Bomb Counter";
                    _tracker.transform.localPosition = TrackerPosition;
                    _tracker.transform.localScale = new(1.3824f, 1.3824f, 1.3824f);
                    _tracker.GetComponent<DisplayItemAmount>().playerDataInt = "BombAmount";
                    _tracker.GetComponent<DisplayItemAmount>().textObject.text = "";
                    _colorless = Object.Instantiate(_tracker.GetComponent<DisplayItemAmount>().textObject.gameObject, _tracker.transform);
                    _colorless.transform.localPosition = new(0.2f, -1f);
                    _colorless.GetComponent<TextMeshPro>().fontSize = 2;
                    _colorless.SetActive(BombManager.ColorlessHelp);
                    _tracker.GetComponent<DisplayItemAmount>().textObject.fontSize = 5;
                    _tracker.GetComponent<DisplayItemAmount>().textObject.gameObject.name = "Counter";
                    _tracker.GetComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
                    _tracker.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 1f);
                    _tracker.GetComponent<BoxCollider2D>().offset = new Vector2(0.5f, 0f);
                    _tracker.SetActive(BombManager.BombBagLevel > 0);
                    _tracker.transform.GetChild(0).localPosition = new(1.234f, -0.531f, 0.0791f);
                }
                catch (System.Exception exception)
                {
                    LogHelper.Write<BomberKnight>("Couldn't create tracker: " + exception.ToString(), KorzUtils.Enums.LogType.Error);
                }
            }
            return _tracker;
        }
    }

    public static Vector3 TrackerPosition { get; set; } = new(-2.1455f, -2.4491f, 0f);

    #endregion

    #region Event handler

    private static int GetPlayerInt_BombAmount(string name, int orig)
    {
        if (name == "BombAmount")
            return BombManager.BombQueue.Count;
        return orig;
    }

    #endregion

    #region Control

    internal static void CreateInventoryPage(GameObject inventoryObject)
    {
        // Prevent multiple calls (The only element that should already been here is the cursor)
        if (inventoryObject.transform.childCount > 1)
            return;
        _controlElements.Clear();
        PlayMakerFSM fsm = inventoryObject.LocateMyFSM("Empty UI");

        _controlElements.Add("Content", new GameObject[30]);

        GameObject bombTitle = Object.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject);
        bombTitle.transform.SetParent(inventoryObject.transform);
        bombTitle.transform.localPosition = new(13.9f, -7.5f, -2f);
        bombTitle.GetComponent<TextMeshPro>().text = "";
        bombTitle.GetComponent<TextContainer>().size = new(6f, 10.258f);
        _controlElements.Add("Title", new GameObject[1] { bombTitle });

        GameObject bombDescription = Object.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
        bombDescription.transform.SetParent(inventoryObject.transform);
        bombDescription.transform.localPosition = new(14.1f, -9f, 1f);
        bombDescription.GetComponent<TextMeshPro>().text = "";
        bombDescription.GetComponent<TextContainer>().size = new(6f, 10.258f);
        _controlElements.Add("Desc", new GameObject[1] { bombDescription });

        GameObject separatorLeft = Object.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Divider L").gameObject);
        separatorLeft.transform.SetParent(inventoryObject.transform);
        separatorLeft.transform.localPosition = new(-2.8f, -8.3055f, 1f);
        separatorLeft.transform.localScale = new(6.6422f, 0.5253f, 1.3674f);

        GameObject separatorRight = Object.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Divider L").gameObject);
        separatorRight.transform.SetParent(inventoryObject.transform);
        separatorRight.transform.localPosition = new(10.32f, -8.3055f, 1f);
        separatorRight.transform.localScale = new(6.6422f, 0.5253f, 1.3674f);

        // Generates all bomb preview objects
        float xPosition = 0f; // in 1,5f steps
        float yPosition = -2.1f; // in -2f steps
        for (int i = 1; i < 31; i++)
        {
            _controlElements["Content"][i - 1] = CreateImageObject(inventoryObject.transform, "Bomb Preview" + i, new(xPosition, yPosition, -3f), new(1.5f, 1.5f, 1.5f), false);
            xPosition += 2f;
            if (xPosition > 8f)
            {
                xPosition = 0f;
                yPosition -= 1.5f;
            }
        }

        GameObject glow = Object.Instantiate(inventoryObject.transform.Find("Cursor/Back/Glow").gameObject);
        Object.Destroy(glow.GetComponent<PlayMakerFSM>());
        glow.transform.SetParent(inventoryObject.transform);
        glow.transform.localPosition = new(0f, -2.1f);
        glow.transform.localScale = new(0.8f, 0.8f, 1f);

        GameObject bombBag = CreateImageObject(inventoryObject.transform, "Bomb Bag", new(4f, -12f, -3f), new(2f, 2f, 1.5f), true);
        bombBag.GetComponentInChildren<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombBag");
        _controlElements.Add("BombBag", new GameObject[1] { bombBag });

        // Create objects for all available bomb types.
        _controlElements.Add("Available", new GameObject[6]);

        xPosition = -7.5f;
        yPosition = -3f;
        for (int i = 0; i < 6; i++)
        {
            _controlElements["Available"][i] = CreateImageObject(inventoryObject.transform, (BombType)i + "", new(xPosition, yPosition, -3f), new(2.5f, 2.5f, 2.5f));
            xPosition += 3f;
            if (xPosition > -4.5f)
            {
                xPosition = -7.5f;
                yPosition -= 4.5f;
            }
        }
        AddInventoryControl(fsm);
        inventoryObject.SetActive(false);
    }

    internal static void StartListening()
    {
        ModHooks.GetPlayerIntHook += GetPlayerInt_BombAmount;
        Tracker.SetActive(BombManager.BombBagLevel > 0);
    }

    internal static void StopListening()
    {
        ModHooks.GetPlayerIntHook -= GetPlayerInt_BombAmount;
    }

    #endregion

    #region Methods

    private static GameObject CreateImageObject(Transform parent, string name, Vector3 position, Vector3 scale, bool selectable = true)
    {
        GameObject gameObject = new(name);
        gameObject.transform.SetParent(parent);
        gameObject.transform.localPosition = position;
        gameObject.transform.localScale = scale;
        gameObject.layer = parent.gameObject.layer;
        if (selectable)
            gameObject.AddComponent<BoxCollider2D>().offset = new(0f, 0f);
        GameObject child = new("Image");
        child.transform.SetParent(gameObject.transform);
        child.transform.localPosition = new(0f, 0f, 0f);
        child.transform.localScale = new(1f, 1f, 1f);
        child.layer = parent.gameObject.layer;
        child.AddComponent<SpriteRenderer>().sprite = _emptySprite;
        child.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
        child.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";
        return gameObject;
    }

    private static void AddInventoryControl(PlayMakerFSM fsm)
    {
        // Add index variable
        List<FsmInt> intVariables = fsm.FsmVariables.IntVariables.ToList();
        FsmInt indexVariable = new() { Name = "ItemIndex", Value = 0 };
        intVariables.Add(indexVariable);
        fsm.FsmVariables.IntVariables = intVariables.ToArray();

        // Removing the jump from arrow button to arrow button.
        fsm.GetState("L Arrow").RemoveTransitionsTo("R Arrow");
        fsm.GetState("R Arrow").RemoveTransitionsTo("L Arrow");

        FsmState currentWorkingState = fsm.GetState("Init Heart Piece");
        currentWorkingState.Name = "Init Bombs";
        currentWorkingState.RemoveTransitionsTo("L Arrow");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            int runs = 0;
            foreach (Transform child in fsm.transform)
            {
                child.gameObject.SetActive(true);
                runs++;
                if (runs == 5)
                    break;
            }
        }));

        // Create main state
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Highlight",
            Actions = new FsmStateAction[]
            {
                    new Lambda(() =>
                    {
                        GameObject destination;
                        if (indexVariable.Value == 6)
                            destination = _controlElements["BombBag"][0];
                        else
                            destination = _controlElements["Available"][indexVariable.Value];

                        fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = destination;
                    }),
                    new SetSpriteRendererOrder()
                    {
                        gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
                        order = 0,
                        delay = 0f
                    },
                    new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                    new Lambda(() =>
                    {
                        _controlElements["Title"][0].GetComponent<TextMeshPro>().text = indexVariable.Value == 6
                        ? InventoryText.BombBag_Title
                        : BombManager.AvailableBombs[(BombType)indexVariable.Value]
                            ? InventoryText.ResourceManager.GetString($"{(BombType)indexVariable.Value}_Title")
                            : "???";

                        _controlElements["Desc"][0].GetComponent<TextMeshPro>().text = indexVariable.Value == 6
                        ? string.Format(InventoryText.BombBag_Desc, BombManager.BombBagLevel * 10)
                        : BombManager.AvailableBombs[(BombType)indexVariable.Value]
                            ? InventoryText.ResourceManager.GetString($"{(BombType)indexVariable.Value}_Desc")
                            : "???";
                        if (indexVariable.Value == 6 && BombManager.ColorlessHelp)
                        {
                            string bombAmount = "";
                            for (int i = 0; i < 5; i++)
                            {
                                string bombType =((BombType)i).ToString().Substring(0, ((BombType)i).ToString().Length - 4);
                                bombAmount += $"{bombType}: {BombManager.BombQueue.Count(x => (int)x == i)}, ";
                            }
                            _controlElements["Desc"][0].GetComponent<TextMeshPro>().text += "\n"+ bombAmount.Trim(new char[]{' ', ','});
                        }
                    })
            }
        });

        // Add transition from init to main
        currentWorkingState.AddTransition("FINISHED", "Highlight");

        currentWorkingState = fsm.GetState("Highlight");

        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Up Press" });
        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Right Press" });
        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Down Press" });
        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Left Press" });
        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Toggle Power" });

        currentWorkingState.AddTransition("UI UP", "Up Press");
        currentWorkingState.AddTransition("UI RIGHT", "Right Press");
        currentWorkingState.AddTransition("UI DOWN", "Down Press");
        currentWorkingState.AddTransition("UI LEFT", "Left Press");

        // Left
        currentWorkingState = fsm.GetState("Left Press");
        currentWorkingState.AddTransition("OUT", "L Arrow");
        currentWorkingState.AddTransition("FINISHED", "Highlight");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            if (indexVariable.Value == 0)
            {
                indexVariable.Value = -2;
                fsm.SendEvent("OUT");
                return;
            }
            else if (indexVariable.Value == -1)
                indexVariable.Value = 6;
            else
                indexVariable.Value--;
            fsm.SendEvent("FINISHED");
        }));
        fsm.GetState("R Arrow").AddTransition("UI LEFT", "Left Press");

        // Right
        currentWorkingState = fsm.GetState("Right Press");
        currentWorkingState.AddTransition("OUT", "R Arrow");
        currentWorkingState.AddTransition("FINISHED", "Highlight");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            if (indexVariable.Value == 6)
            {
                indexVariable.Value = -1;
                fsm.SendEvent("OUT");
                return;
            }
            indexVariable.Value = indexVariable.Value == -2
            ? 0
            : indexVariable.Value + 1;
            fsm.SendEvent("FINISHED");
        }));
        fsm.GetState("L Arrow").AddTransition("UI RIGHT", "Right Press");

        // Up
        currentWorkingState = fsm.GetState("Up Press");
        currentWorkingState.AddTransition("FINISHED", "Highlight");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            if (indexVariable.Value < 6 && indexVariable.Value >= 0)
                indexVariable.Value = indexVariable.Value < 2
                ? indexVariable.Value + 4
                : indexVariable.Value - 2;
            fsm.SendEvent("FINISHED");
        }));

        // Down
        currentWorkingState = fsm.GetState("Down Press");
        currentWorkingState.AddTransition("FINISHED", "Highlight");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            if (indexVariable.Value < 6 && indexVariable.Value >= 0)
                indexVariable.Value = indexVariable.Value < 4
                ? indexVariable.Value + 2
                : indexVariable.Value - 4;
            fsm.SendEvent("FINISHED");
        }));
    }

    /// <summary>
    /// Updates the inventory page to show the correct amount of bombs, available bomb etc.
    /// </summary>
    public static void UpdateBombPage()
    {
        try
        {
            for (int i = 0; i < _controlElements["Available"].Length; i++)
            {
                GameObject currentBomb = _controlElements["Available"][i];
                if (BombManager.AvailableBombs[(BombType)i])
                {
                    currentBomb.GetComponentInChildren<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
                    currentBomb.GetComponentInChildren<SpriteRenderer>().color = BombManager.GetBombColor((BombType)i);
                }
                else
                {
                    currentBomb.GetComponentInChildren<SpriteRenderer>().sprite = _emptySprite;
                    currentBomb.GetComponentInChildren<SpriteRenderer>().color = Color.white;
                }
            }

            // Display bomb bag content
            for (int i = 0; i < _controlElements["Content"].Length; i++)
            {
                // Empty sprites need to be moved +0.1x and -0.2y to align with the sprites.
                GameObject currentBomb = _controlElements["Content"][i];
                if (i >= BombManager.BombBagLevel * 10)
                {
                    currentBomb.SetActive(false);
                    continue;
                }
                currentBomb.SetActive(true);
                if (BombManager.BombQueue.Count < i + 1)
                {
                    currentBomb.GetComponentInChildren<SpriteRenderer>().sprite = _emptySprite;
                    currentBomb.GetComponentInChildren<SpriteRenderer>().color = Color.white;

                }
                else
                {
                    currentBomb.GetComponentInChildren<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<BomberKnight>("Sprites.BombSprite");
                    currentBomb.GetComponentInChildren<SpriteRenderer>().color = BombManager.GetBombColor(BombManager.BombQueue[i]);
                }
            }
        }
        catch (System.Exception exception)
        {
            LogHelper.Write<BomberKnight>("An error occurred while trying to update the inventory page: " + exception, KorzUtils.Enums.LogType.Error);
        }
    }

    /// <summary>
    /// Updates the tracker that shows the bomb amount and the next bomb.
    /// </summary>
    public static void UpdateTracker()
    {
        try
        {
            if (BombManager.BombQueue.Count > 0)
                Tracker.GetComponent<SpriteRenderer>().color = BombManager.GetBombColor(BombManager.BombQueue[0]);
            else
                Tracker.GetComponent<SpriteRenderer>().color = Color.white;
            Tracker.GetComponent<DisplayItemAmount>().textObject.text = BombManager.BombQueue.Count.ToString();
            Tracker.SetActive(BombManager.BombBagLevel > 0);
            if (BombManager.ColorlessHelp)
            {
                if (_colorless == null)
                {
                    _colorless = Object.Instantiate(_tracker.GetComponent<DisplayItemAmount>().textObject.gameObject, _tracker.transform);
                    _colorless.transform.localPosition = new(0.2f, -1f);
                    _colorless.GetComponent<TextMeshPro>().fontSize = 2;
                }
                _colorless.SetActive(true);
                if (BombManager.BombQueue.Count > 0)
                    _colorless.GetComponent<TextMeshPro>().text = BombManager.BombQueue[0].ToString().Substring(0, BombManager.BombQueue[0].ToString().Length - 4);
                else
                    _colorless.GetComponent<TextMeshPro>().text = "";
            }
            else if (_colorless != null && _colorless.activeSelf)
                _colorless.SetActive(false);
            Tracker.transform.localPosition = TrackerPosition;
            Tracker.transform.localScale = new(1.3824f, 1.3824f, 1.3824f);
        }
        catch (System.Exception exception)
        {
            LogHelper.Write<BomberKnight>("Failed to update bomb tracker: " + exception, KorzUtils.Enums.LogType.Error);
        }
    }

    #endregion
}