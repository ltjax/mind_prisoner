using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const int MINIMAP_SIZE = 5;
    Text hint;
    List<Image> minimapPanel = new List<Image>();
    RoomManager roomManager;
    Coroutine currentText;

    // Start is called before the first frame update
    void Start()
    {
        roomManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<RoomManager>();
        hint = GetComponentInChildren<Text>();
        for (int y = 0; y < MINIMAP_SIZE; ++y)
        {
            for (int x = 0; x < MINIMAP_SIZE; ++x)
            {
                var name = string.Format("UI/Minimap/Room{0}{1}", x, y);
                var image = GameObject.Find(name).GetComponent<Image>();
                Debug.Assert(image != null);
                image.enabled = false;
                minimapPanel.Add(image);
            }
        }
        Debug.Assert(hint != null);
        UpdateMinimap(Vector2Int.zero);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ShowText(string text)
    {
        if (currentText != null)
        {
            StopCoroutine(currentText);
        }

        currentText = StartCoroutine(TextAnimation(text));
    }

    IEnumerator TextAnimation(string text)
    {
        hint.color = Color.white;
        hint.text = "";
        for (int i = 0; i < text.Length; ++i)
        {
            hint.text += text[i];
            yield return new WaitForSecondsRealtime(0.01f);
        }
        yield return new WaitForSecondsRealtime(4.0f);
        for (float alpha = 1.0f; alpha > 0.0f; alpha -= 0.03f)
        {
            var color = hint.color;
            color.a = alpha;
            hint.color = color;
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }

    public void LetGo()
    {
        ShowText("As I let go of my past, I feel like new paths are opening up!");
    }

    public void StillThingsToDo()
    {
        switch (Random.Range(0, 3))
        {
            default:
            case 0:
                ShowText("I feel like I still have unresolved business there...");
                break;
            case 1:
                ShowText("There's still something I have to do there...");
                break;
            case 2:
                ShowText("I'm not ready to let go yet...");
                break;
        }
    }

    public void NothingThere()
    {
        switch (Random.Range(0, 3))
        {
            default:
            case 0:
                ShowText("Does not look like anything to me.");
                break;
            case 1:
                ShowText("There's nothing there.");
                break;
            case 2:
                ShowText("What..?!");
                break;
        }
    }

    public void UpdateMinimap(Vector2Int center)
    {
        var offset = MINIMAP_SIZE / 2;
        for (int y = 0; y < MINIMAP_SIZE; ++y)
        {
            for (int x = 0; x < MINIMAP_SIZE; ++x)
            {
                var index = y * MINIMAP_SIZE + x;
                var cell = center + new Vector2Int(x - offset, y - offset);
                var state = roomManager.CheckRoom(cell);
                var panel = minimapPanel[index];
                var alpha = cell == center ? 0.9f : 0.45f;
                switch (state)
                {
                    case RoomManager.RoomState.NonExistant:
                        panel.enabled = false;
                        break;
                    case RoomManager.RoomState.Freed:
                        panel.enabled = true;
                        panel.color = new Color(1.0f, 1.0f, 1.0f, alpha);
                        break;
                    case RoomManager.RoomState.Hostiles:
                        panel.enabled = true;
                        panel.color = new Color(1.0f, 0.2f, 0.2f, alpha);
                        break;
                    case RoomManager.RoomState.Unvisited:
                        panel.enabled = true;
                        panel.color = new Color(1.0f, 1.0f, 0.8f, 0.2f);
                        break;
                }
            }
        }
    }
}
